module Index

open Elmish
open SAFE
open Shared

let habitEmojis = [
    "🏋️‍♂️"; "🏃‍♀️"; "🚴‍♂️";  // Exercise
    "🥦"; "🍎"; "🍉";          // Healthy Eating
    "🧘‍♂️"; "☕"; "🌱";        // Mindfulness
    "📚"; "✏️"; "📖";          // Reading
    "👫"; "🤝"; "🎉";          // Social Habits
    "✅"; "📝"; "⏰"           // Productivity
]

type InputCard = { Emoji: string; Habit: string }

type Model = {
    IsCardHovered: bool
    IsFormOpen: bool 
    IsEmojiDropDownOpen: bool
    InputCard: InputCard option
}

type Msg =
    | SetCardHover of bool
    | SetFormOpen of bool
    | SetEmojiDropDown of bool
    | SaveInputCard of string * string

let todosApi = Api.makeProxy<IHabitsApi> ()

let init () =
    let initialModel = { IsCardHovered = false; IsFormOpen = false; IsEmojiDropDownOpen = false; InputCard = None}
    let initialCmd = Cmd.none

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetCardHover isHovered -> { model with IsCardHovered = isHovered }, Cmd.none
    | SetFormOpen isOpen ->
        match isOpen with
        | true -> { model with IsFormOpen = true }, Cmd.none
        | false -> { model with IsFormOpen = false; InputCard = None }, Cmd.none
    | SetEmojiDropDown isOpen -> { model with IsEmojiDropDownOpen = isOpen }, Cmd.none
    | SaveInputCard (emoji, habit) -> 
        let newCard = { Emoji = emoji; Habit = habit }
        {model with InputCard = Some newCard },Cmd.none

open Feliz
open Feliz.DaisyUI

module ViewComponents =
    let InputCardEmoji model dispatch =
        let emojis = habitEmojis

        let selectEmoji emoji = dispatch (SaveInputCard (emoji, ""))

        Html.div [
            prop.className "relative justify-self-start self-center ml-10"
            prop.children [
                Html.div [
                    prop.className "text-6xl flex items-center justify-center"           
                    prop.onClick ( fun _ -> dispatch (SetEmojiDropDown (not model.IsEmojiDropDownOpen)) )     
                    prop.children [
                        match model.InputCard with
                            | Some card -> Html.p card.Emoji
                            | None -> 
                                Html.button [
                                    prop.className "btn btn-neutral flex items-center"
                                    prop.children [
                                        Html.i [ prop.className "fas fa-upload mr-2" ]
                                        Html.span [ prop.text "Select" ]
                                    ]
                                ]
                    ]
                ]

                // Emoji Dropdown
                match model.IsEmojiDropDownOpen with
                    | true -> 
                        Html.div [
                            prop.className "absolute z-10 bg-white shadow-lg border border-gray-300 mt-1 rounded"
                            prop.children [
                                for emoji in emojis do
                                    Html.div [
                                        prop.className "p-2 cursor-pointer hover:bg-gray-100 text-xl"
                                        prop.text emoji
                                        prop.onClick (fun _ -> selectEmoji emoji; dispatch (SetEmojiDropDown (not model.IsEmojiDropDownOpen))) // Select emoji on click
                                    ]
                            ]
                        ]
                    | _ -> ()
            ]
        ]
    let InputCardHabit model dispatch =
        let currentHabit =
            match model.InputCard with 
            | Some card -> card.Habit
            | None -> ""
        
        let isEmojiSelected = 
            match model.InputCard with
            | Some card when card.Emoji <> "" -> true
            | _ -> false

        Html.div [
            prop.className "flex items-center justify-center flex-auto"
            prop.children [
                Html.input [
                    prop.className "input input-bordered w-full max-w-xs"
                    prop.placeholder "Enter your habit name"
                    prop.value currentHabit
                    prop.disabled (not isEmojiSelected)
                    prop.onChange (fun newValue ->
                        match model.InputCard with
                        | Some card -> dispatch (SaveInputCard (card.Emoji, newValue))
                        | None -> dispatch (SaveInputCard ("", newValue))
                    )
                ]
                Html.img [
                    prop.className "justify-self-end self-start cursor-pointer"
                    prop.src "/prev-icon.svg"
                    prop.alt "go back"
                    prop.onClick (fun ev ->
                        ev.stopPropagation()
                        dispatch (SetFormOpen false)
                    )
                ]
                Html.img [
                    prop.className "justify-self-end self-end cursor-pointer"
                    prop.src "/next-icon.svg"
                    prop.alt "go next"
                    prop.onClick (fun ev ->
                        ev.stopPropagation()
                        dispatch (SetFormOpen false)
                    )
                ]
            ]
        ]

    let InputCardView model dispatch = 
        Html.div [
            prop.className ("h-40 w-4/12 rounded shadow-xl flex bg-chineseviolet" + 
                (if model.IsCardHovered && not model.IsFormOpen then " hover:bg-mountbattenpink" else ""))
            prop.onMouseEnter (fun _ -> dispatch (SetCardHover true))
            prop.onMouseLeave (fun _ -> dispatch (SetCardHover false))
            prop.onClick (fun _ -> dispatch (SetFormOpen true))
            prop.children [
                match model.IsFormOpen with
                | true -> 
                    InputCardEmoji model dispatch
                    InputCardHabit model dispatch
                | false ->
                    Html.div [
                        prop.className "flex justify-center w-full cursor-pointer"
                        prop.children [
                            Html.img [
                                prop.src "/add-icon.svg"
                                prop.alt "Add Your Habit"
                            ]
                        ]
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
                    ViewComponents.InputCardView model dispatch
                ]
            ]
        ]
    ]