namespace Shared

open System

type Habit = { Emoji: string; Habit: string }

type IHabitsApi = {
    getHabits: unit -> Async<Habit list>
    //addHabits: Habit -> Async<Habit>
}













// type Todo = { Id: Guid; Description: string }

// module Todo =
//     let isValid (description: string) =
//         String.IsNullOrWhiteSpace description |> not

//     let create (description: string) = {
//         Id = Guid.NewGuid()
//         Description = description
//     }

// type ITodosApi = {
//     getTodos: unit -> Async<Todo list>
//     addTodo: Todo -> Async<Todo>
// }