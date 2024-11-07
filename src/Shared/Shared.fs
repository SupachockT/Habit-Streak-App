namespace Shared

open System

type Habit = {
    Id: int
    Emoji: string
    Name: string
    StreakDate: DateTime option
    Streak: int
    isDone: bool
}

type InputCard = { Emoji: string; Habit: string }

type IHabitsApi = {
    getAllHabits: unit -> Async<Habit list>
    createHabit: InputCard -> Async<int>
    updateStreak: int * bool -> Async<bool>
    deleteHabit: int -> Async<bool>
}