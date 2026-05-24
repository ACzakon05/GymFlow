# GymFlow

GymFlow to aplikacja ASP.NET Core MVC do zarządzania treningami, ćwiczeniami i postępem na siłowni. Projekt zawiera zarówno klasyczny interfejs webowy, jak i REST API z prostą autoryzacją opartą na `username` oraz `ApiKey`.

## Główne funkcje

- Rejestracja i logowanie użytkowników
- Role `Admin` i `User`
- CRUD dla ćwiczeń, treningów oraz wpisów postępu
- Możliwość dodawania ćwiczeń do treningów (relacja N:M)
- REST API z autentykacją nagłówków `X-Api-Username` i `X-Api-Key`
- Dokumentacja Swagger

## Modele danych

- `User` - konto użytkownika: `Id`, `Username`, `Email`, `PasswordHash`, `FirstName`, `LastName`, `Role`, `ApiKey`, `CreatedAt`, `IsActive`
- `ExerciseCategory` - kategorie ćwiczeń: `Id`, `Name`, `Description`, `CreatedAt`
- `Exercise` - ćwiczenie: `Id`, `Name`, `Description`, `ExerciseCategoryId`, `MuscleGroup`, `Difficulty`, `CreatedAt`
- `Workout` - trening: `Id`, `Name`, `Description`, `UserId`, `CreatedAt`, `UpdatedAt`, `IsActive`
- `WorkoutExercise` - relacja trening-ćwiczenie: `Id`, `WorkoutId`, `ExerciseId`, `Sets`, `Reps`, `Weight`, `RestSeconds`, `Notes`, `Order`
- `ProgressEntry` - wpis postępu: `Id`, `UserId`, `ExerciseId`, `Date`, `CompletedSets`, `CompletedReps`, `ActualWeight`, `IsCompleted`, `Notes`, `Resistance`

## Struktura projektu

- `/Controllers` - kontrolery MVC
- `/Controllers/Api` - kontrolery REST API
- `/Data/ApplicationDbContext.cs` - EF Core DbContext
- `/Models` - modele danych
- `/Views` - widoki Razor
- `/wwwroot` - zasoby statyczne

## Jak uruchomić

1. Otwórz terminal w katalogu `GymFlow`
2. Uruchom:

```bash
dotnet run
```

3. W przeglądarce otwórz stronę aplikacji, np. `http://localhost:5000`

## REST API

### Autoryzacja

Wszystkie wywołania API wymagają autoryzacji nagłówkami:

- `X-Api-Username: <username>`
- `X-Api-Key: <api-key>`

Alternatywnie można użyć query stringów:

- `?username=<username>&apikey=<api-key>`

### Dostępne endpointy

- `GET /api/exercises`
- `GET /api/exercises/{id}`
- `POST /api/exercises`
- `PUT /api/exercises/{id}`
- `DELETE /api/exercises/{id}`

- `GET /api/workouts`
- `GET /api/workouts/{id}`
- `POST /api/workouts`
- `PUT /api/workouts/{id}`
- `DELETE /api/workouts/{id}`

- `GET /api/progress`
- `GET /api/progress/{id}`
- `POST /api/progress`
- `PUT /api/progress/{id}`
- `DELETE /api/progress/{id}`

- `GET /api/exercisecategories`
- `GET /api/exercisecategories/{id}`
- `POST /api/exercisecategories`
- `PUT /api/exercisecategories/{id}`
- `DELETE /api/exercisecategories/{id}`

- `GET /api/workoutexercises`
- `GET /api/workoutexercises/{id}`
- `POST /api/workoutexercises`
- `PUT /api/workoutexercises/{id}`
- `DELETE /api/workoutexercises/{id}`

- `GET /api/users/me`
- `PUT /api/users/me`

### Przykładowe wywołanie cURL

```bash
curl -H "X-Api-Username: admin" \
     -H "X-Api-Key: <your-api-key>" \
     http://localhost:5000/api/exercises
```

## Swagger

Po uruchomieniu aplikacji dostępny jest Swagger UI pod adresem:

- `http://localhost:5000/swagger`

## Baza danych

Aplikacja używa SQLite. Domyślny plik bazy znajduje się w katalogu projektu jako `gymflow.db`.

## Uwagi

- Hasła są przechowywane jako hash BCrypt
- `ApiKey` jest generowany dla użytkownika przy tworzeniu konta lub przy starcie, jeśli brakowało wartości
- REST API obsługuje tylko aktywnych użytkowników (`IsActive = true`)
