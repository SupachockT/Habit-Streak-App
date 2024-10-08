module Index

open Elmish
open SAFE
open Shared

type Model = {
    Todos: RemoteData<Todo list>
    Input: string
    IsModalOpen : bool
}

type Msg =
    | SetInput of string
    | LoadTodos of ApiCall<unit, Todo list>
    | SaveTodo of ApiCall<string, Todo>
    | OpenModal
    | CloseModal

let todosApi = Api.makeProxy<ITodosApi> ()

let init () =
    let initialModel = { Todos = NotStarted; Input = ""; IsModalOpen = false }
    let initialCmd = LoadTodos(Start()) |> Cmd.ofMsg

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetInput value -> { model with Input = value }, Cmd.none
    | LoadTodos msg ->
        match msg with
        | Start() ->
            let loadTodosCmd = Cmd.OfAsync.perform todosApi.getTodos () (Finished >> LoadTodos)

            { model with Todos = Loading }, loadTodosCmd
        | Finished todos -> { model with Todos = Loaded todos }, Cmd.none
    | SaveTodo msg ->
        match msg with
        | Start todoText ->
            let saveTodoCmd =
                let todo = Todo.create todoText
                Cmd.OfAsync.perform todosApi.addTodo todo (Finished >> SaveTodo)

            { model with Input = "" }, saveTodoCmd
        | Finished todo ->
            {
                model with
                    Todos = model.Todos |> RemoteData.map (fun todos -> todos @ [ todo ])
            },
            Cmd.none
    | OpenModal -> { model with IsModalOpen = true }, Cmd.none
    | CloseModal -> { model with IsModalOpen = false }, Cmd.none

open Feliz
open Feliz.DaisyUI

module ViewComponents =
    let todoAction model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row mt-4 gap-4"
            prop.children [
                Html.input [
                    prop.className
                        "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                    prop.value model.Input
                    prop.placeholder "What needs to be done?"
                    prop.autoFocus true
                    prop.onChange (SetInput >> dispatch)
                    prop.onKeyPress (fun ev ->
                        if ev.key = "Enter" then
                            dispatch (SaveTodo(Start model.Input)))
                ]
                Html.button [
                    prop.className
                        "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                    prop.disabled (Todo.isValid model.Input |> not)
                    prop.onClick (fun _ -> dispatch (SaveTodo(Start model.Input)))
                    prop.text "Add"
                ]
            ]
        ]

    let todoList model dispatch =
        Html.div [
            prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
            prop.children [
                Html.ol [
                    prop.className "list-decimal ml-6"
                    prop.children [
                        match model.Todos with
                        | NotStarted -> Html.text "Not Started."
                        | Loading -> Html.text "Loading..."
                        | Loaded todos ->
                            for todo in todos do
                                Html.li [ prop.className "my-1"; prop.text todo.Description ]
                    ]
                ]

                todoAction model dispatch
            ]
        ]

let view model dispatch =
    Html.section [
        prop.className "relative h-screen w-screen bg-cp1"

        prop.children [
            Html.h1 [
                prop.className "flex justify-center py-6 font-semibold text-3xl text-cp4"
                prop.text "HABIT STREAK APP!"
            ]

            //Button
            Html.div [
                prop.className "p-4 absolute bottom-0 right-0"
                prop.children [
                    Daisy.button.button [
                        prop.className "bg-cp3 text-xl text-cp2"
                        prop.text "Add"
                        button.circle
                        button.lg
                        prop.onClick (fun _ -> dispatch OpenModal)
                    ]
                ]
            ]

            //Modal
            if model.IsModalOpen then
                Html.div [
                    prop.className "modal modal-open"
                    prop.children [
                        Html.div [
                            prop.className "modal-box relative"
                            prop.children [
                                Html.h3 [ prop.className "font-bold text-lg flex justify-center"; prop.text "Add a New Habit" ]

                                // Button to close the modal
                                Html.button [
                                    prop.className "absolute top-0 right-2 text-red-500 font-bold text-xl"
                                    prop.text "✕"
                                    prop.onClick (fun _ -> dispatch CloseModal)
                                ]
                            ]
                        ]
                    ]
                ]
        ]
    ]

            // Html.div [
            //     prop.className "flex flex-col items-center justify-center h-full"
            //     prop.children [
            //         ViewComponents.todoList model dispatch
            //     ]
            // ]