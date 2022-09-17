![Build & Tests](https://github.com/Christian-Nunnally/Structure/actions/workflows/build.yml/badge.svg?branch=main)
# Structure

Strucuture is a tiny, modular, CLI application for tracking stuff.

## How to: Install
1. Clone this repo
> `git clone https://github.com/Christian-Nunnally/Structure.git; cd Structure`

2. Build Structure
> `dotnet build -c Debug`

3. Run Structure
> `.\Structure\bin\Debug\net6.0\Structure.exe`

## How to: Use

[More detailed documentation on the wiki](https://github.com/Christian-Nunnally/Structure/wiki)

## How to: Run tests
> `dotnet test --verbosity normal`

## Current features
- Quickly create and complete task items in a task tree.
- Task lists can be infinitely nested.
- Tasks can be easily reprioitized or moved up/down the task tree. 
- Create 'routines' which are templates of task trees.
- Ability to create ad-hoc focus lists of tasks and switch between tasks in that list.
- Ability to create custom graphs that query from completed tasks.
- Closing and opening application resumes where it was left.
- Ability to start any activity without losing current context.
- All actions performed with single keystrokes.
