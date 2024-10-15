module Index

open Elmish
open SAFE
open Shared

type Model = {
    Todos: RemoteData<Todo list>
    Input: string
    IsCardHovered: bool
    InputCard: InputCard option
}

type Msg =
    | SetInput of string
    | LoadTodos of ApiCall<unit, Todo list>
    | SaveTodo of ApiCall<string, Todo>
    | SetCardHover of bool
    | SaveInputCard of string * string

let todosApi = Api.makeProxy<ITodosApi> ()

let init () =
    let initialModel = { Todos = NotStarted; Input = ""; IsCardHovered = false; InputCard = None }
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
    | SetCardHover isHovered -> { model with IsCardHovered = isHovered }, Cmd.none
    | SaveInputCard (image, name) -> 
        let newCard = { Image = image; Name = name }
        {model with InputCard = Some newCard },Cmd.none

open Feliz
open Feliz.DaisyUI

module ViewComponents =
    let todoAction model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row gap-4 mt-4"
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

    let InputCard model dispatch = 
        Html.div [
            prop.className ("h-4/12 w-4/12 rounded shadow-xl flex items-center justify-center bg-chineseviolet cursor-pointer" + (if model.IsCardHovered then " hover:bg-mountbattenpink" else ""))
            prop.onMouseEnter (fun _ -> dispatch (SetCardHover true))
            prop.onMouseLeave (fun _ -> dispatch (SetCardHover false))
            prop.children [
                Html.img [
                    prop.src "./public/add-icon.svg"
                    prop.alt "Add Your Habit"
                    prop.className "w-48 h-48"
                ]
            ]
        ]

let view model dispatch =
    Html.section [
        prop.className "h-screen w-screen bg-davygray"

        prop.children [
            Html.h1 [
                prop.className "py-6 mb-10 flex justify-center font-semibold text-3xl text-paledogwood"
                prop.text "HABIT STREAK APP!"
            ]

            //Container
            Html.div [
                prop.className "p-4 flex flex-col items-center gap-4 border-dashed border-2 border-sage"
                prop.children [
                    ViewComponents.InputCard model dispatch
                ]
            ]
        ]
    ]