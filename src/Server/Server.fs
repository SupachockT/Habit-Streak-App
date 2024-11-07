module Server

open SAFE
open Saturn
open Shared
open System
open Npgsql.FSharp

let connectionString = "Host=localhost; Database=postgres; Username=postgres; Password=123456;"

module Storage =
    let getAllHabits () = async {
        let! result = 
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT * FROM habits"
            |> Sql.executeAsync (fun read -> 
                {
                    Id = read.int "id"
                    Emoji = read.string "emoji"
                    Name = read.string "name"
                    StreakDate = read.dateTimeOrNone "streak_date"
                    Streak = read.int "streak"
                    isDone = read.bool "isdone"  // Assign value for isDone here
                })
            |> Async.AwaitTask
        return result |> List.ofSeq
    }

    let createHabit (input: InputCard) : Async<int> = async {
        let! ids =
            connectionString
            |> Sql.connect
            |> Sql.query "INSERT INTO habits (emoji, name, streak_date, streak, isdone) VALUES (@emoji, @name, @streakDate, @streak, @isDone) RETURNING id"
            |> Sql.parameters [
                "emoji", Sql.string input.Emoji
                "name", Sql.string input.Habit
                "streakDate", Sql.date (DateTime.Now)
                "streak", Sql.int 0
                "isDone", Sql.bool false 
            ]
            |> Sql.executeAsync (fun read -> read.int "id")  // This returns the id
            |> Async.AwaitTask

        return List.head ids  // Return the first id from the list (as the inserted record ID)
    }

    let updateStreak (id: int) (increment: bool) = async {
        let query = 
            if increment then
                "UPDATE habits SET streak = streak + 1, streak_date = CURRENT_DATE, isdone = TRUE WHERE id = @id"
            else
                "UPDATE habits SET streak = streak - 1, isdone = FALSE WHERE id = @id"

        let! rowsAffected = 
            connectionString
            |> Sql.connect
            |> Sql.query query
            |> Sql.parameters [ "id", Sql.int id ]
            |> Sql.executeNonQueryAsync
            |> Async.AwaitTask

        return rowsAffected > 0  // Returns true if the update was successful
    }

    let deleteHabit (id: int) = async {
        let! rowsAffected = 
            connectionString
            |> Sql.connect
            |> Sql.query "DELETE FROM habits WHERE id = @id"
            |> Sql.parameters [ "id", Sql.int id ]
            |> Sql.executeNonQueryAsync
            |> Async.AwaitTask
        return rowsAffected > 0 
    }

let habitApi ctx = {
    getAllHabits = fun () -> async {
        let! habits = Storage.getAllHabits ()
        return habits
    }
    createHabit = fun input -> async {
        let! habit = Storage.createHabit input
        return habit
    }
    updateStreak = fun (id, increment) -> async {
        let! success = Storage.updateStreak id increment
        return success
    }
    deleteHabit = fun id -> async {
        let! success = Storage.deleteHabit id
        return success
    }
}

let webApp = Api.make habitApi

let app = application {
    use_router webApp
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0
