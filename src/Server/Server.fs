module Server

open SAFE
open Saturn
open Shared
open Dapper
open Npgsql
open System.Data
open System.Threading.Tasks

let connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=123456"
let getConnection() : IDbConnection = new NpgsqlConnection(connectionString)

module Storage =
    let mutable habits: Habit list = 
        async {
            use connection = getConnection()
            let! fetchedHabits = connection.QueryAsync<Habit>("SELECT emoji, habit FROM mydb") |> Async.AwaitTask
            return fetchedHabits |> Seq.toList
        } |> Async.RunSynchronously

let habitApi ctx = {
     getHabits = fun () -> async { return Storage.habits }
    //addHabits = fun _ -> async { return { Emoji = ""; Habit = "" } } // Placeholder for adding habits
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

// module Storage =
//     let todos =
//         ResizeArray [
//             Todo.create "Create new SAFE project"
//             Todo.create "Write your app"
//             Todo.create "Ship it!!!"
//         ]

//     let addTodo todo =
//         if Todo.isValid todo.Description then
//             todos.Add todo
//             Ok()
//         else
//             Error "Invalid todo"

// let todosApi ctx = {
//     getTodos = fun () -> async { return Storage.todos |> List.ofSeq }
//     addTodo =
//         fun todo -> async {
//             return
//                 match Storage.addTodo todo with
//                 | Ok() -> todo
//                 | Error e -> failwith e
//         }
// }