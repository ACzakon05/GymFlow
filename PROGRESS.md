# GymFlow - System Zarządzania Treningami i Siłownią

## 📋 Postęp Projektu

### ✅ ETAP 1: Projekt i Konfiguracja (GOTOWE)
- [x] Stworzony projekt ASP.NET Core MVC (.NET 8.0)
- [x] Zainstalowany Entity Framework Core 8.0 dla SQLite
- [x] Zainstalowane EF Tools do migracji

**Pliki:** 
- GymFlow.csproj
- Program.cs - konfiguracja Di
- appsettings.json - connection string

---

### ✅ ETAP 2: Modele Danych (GOTOWE)

Stworzone 6 modeli w `/Models/`:

1. **User.cs** - Użytkownik
   - Pola: Id, Username, Email, PasswordHash, FirstName, LastName, Role (Enum), ApiKey, CreatedAt, IsActive
   - Relacje: Workouts (1:N), ProgressEntries (1:N)

2. **ExerciseCategory.cs** - Kategoria ćwiczeń
   - Pola: Id, Name (unique), Description, CreatedAt
   - Relacje: Exercises (1:N)

3. **Exercise.cs** - Ćwiczenie
   - Pola: Id, Name, Description, ExerciseCategoryId (FK), MuscleGroup, Difficulty (Enum), CreatedAt
   - Enum: ExerciseDifficulty (Easy=0, Medium=1, Hard=2)
   - Relacje: ExerciseCategory, WorkoutExercises (1:N), ProgressEntries (1:N)

4. **Workout.cs** - Trening
   - Pola: Id, Name, Description, UserId (FK), CreatedAt, UpdatedAt, IsActive
   - Relacje: User, WorkoutExercises (1:N)

5. **WorkoutExercise.cs** - Relacja N:M (Ćwiczenie w treningu)
   - Pola: Id, WorkoutId (FK), ExerciseId (FK), Sets, Reps, Weight, RestSeconds, Notes, Order
   - Relacje: Workout, Exercise

6. **ProgressEntry.cs** - Śledzenie postępu
   - Pola: Id, UserId (FK), ExerciseId (FK), Date, CompletedSets, CompletedReps, ActualWeight, IsCompleted, Notes, Resistance
   - Relacje: User, Exercise

**Wyjaśnione koncepty:**
- `[Key]`, `[Required]`, `[StringLength]`, `[Display]`, `[ForeignKey]`
- `virtual` - dla lazy loading i change tracking w EF Core

---

### ✅ ETAP 3: Baza Danych (GOTOWE)

1. **Stworzony ApplicationDbContext.cs** w `/Data/`
   - Konfiguracja wszystkich DbSet
   - OnModelCreating - ustawienie relacji, indeksów, cascade delete
   - Foreign keys z odpowiednimi strategiami delete

2. **Skonfigurowany Program.cs**
   - AddDbContext - rejestracja EF Core z SQLite
   - Connection string: `Data Source=gymflow.db`

3. **Zainstalowany dotnet-ef** - narzędzie CLI

4. **Migracja InitialCreate** - w `/Migrations/`
   - Stworzenie wszystkich 6 tabel
   - Stworzenie indeksów (unique na Username, Email)
   - Stworzenie Foreign Keys z CASCADE/RESTRICT delete

5. **Baza danych SQLite** - `gymflow.db` (84KB)
   - ✅ Gotowa do użytku!

---

## 📅 PLANY NA JUTRO (ETAP 4+)

### ETAP 4: Autoryzacja (Logowanie/Rejestracja)
- [X] AuthController.cs - rejestracja i logowanie
- [X] Hashowanie haseł - BCrypt lub Identity
- [X] Views - Login i Register
- [X] Sessions / Cookies

### ETAP 5: CRUD dla Ćwiczeń
- [X] ExercisesController.cs
- [X] Views do managementu ćwiczeń

### ETAP 6: CRUD dla Treningów
- [X] WorkoutsController.cs
- [X] Dodawanie ćwiczeń do treningów

### ETAP 7: Śledzenie Postępu
- [X] ProgressController.cs
- [X] Dashboard ze statystykami

### ETAP 8: Admin Panel
- [X] AdminController.cs - zarządzanie użytkownikami, ćwiczeniami, treningami
- [X] Role-based access control (Admin vs User)
- [X] Widoki admina
- [X] Logi aktywności

### ETAP 8: REST API
- [ ] ApiController - endpoints JSON
- [ ] Autoryzacja przez API Key
- [ ] Swagger/OpenAPI documentation

---

## 🏗️ Struktura Projektu

```
GymFlow/
├── Controllers/
│   ├── HomeController.cs        
│   ├── AuthController.cs        
│   ├── ExercisesController.cs   
│   ├── WorkoutsController.cs    
│   ├── ProgressController.cs  
|   ├── AdminController.cs
│   └── Api/
│       └── ApiController.cs     (TODO)
├── Models/
│   ├── User.cs                  ✅
│   ├── ExerciseCategory.cs      ✅
│   ├── Exercise.cs              ✅
│   ├── Workout.cs               ✅
│   ├── WorkoutExercise.cs       ✅
│   └── ProgressEntry.cs         ✅
├── Data/
│   └── ApplicationDbContext.cs   ✅
├── Views/                       (TODO - formularze MVC)
├── Migrations/
│   └── 20260516004142_InitialCreate.cs ✅
├── Program.cs                   ✅
├── appsettings.json             ✅
├── GymFlow.csproj               ✅
├── gymflow.db                   ✅ (SQLite)
└── wwwroot/                     (CSS, JS - TODO)
```

---

## 🔑 Ważne Hasła do Zapamiętania

1. **DbContext** - klasa łącząca modele z bazą
2. **Migrations** - historia zmian schematu bazy
3. **Foreign Keys (FK)** - łączą tabele między sobą
4. **Relacje:**
   - 1:N - jeden użytkownik ma wiele treningów
   - N:M - wiele ćwiczeń w wielu treningach (przez WorkoutExercise)
5. **DELETE CASCADE** - kasowanie użytkownika kasuje jego treningi
6. **DELETE RESTRICT** - nie możesz skasować ćwiczenia które jest w treningu

---

## 🚀 Następny Krok

Zaczynamy **ETAP 4: Autoryzacja**
- Logowanie i rejestracja
- Hashowanie haseł (BCrypt)
- Session management
- Login form w MVC Views

---

**Data Aktualizacji:** 16.05.2026
