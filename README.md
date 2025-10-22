# Webapplikasjon for registrering av luftfartshindre

**Laget av Gruppe 1 best�ende av:** Adam Elmasry, Benjamin Fidjeland, Hans Kristian Steffenstorpet, Kasper Rustad, Kristian Stenersen & Mats Lie.

## 1. Driftsdokumentasjon: Slik kj�rer du applikasjonen

Prosjektet er satt opp for kj�ring via **Docker** for � sikre et portabelt milj�. Denne metoden sikrer at Node.js, .NET SDK, og alle avhengigheter er riktig konfigurert og tilgjengelige i containeren.

**N�dvendige verkt�y:**

* **Docker Desktop** (m� kj�re i bakgrunnen)
* **Visual Studio** (Med Container Development Workloads)

**Stegvis kj�ring:**

1.  **Klone Prosjektet:** Klon dette Git-repositoriet.
2.  **�pne L�sningen:** �pne `FirstWebApplication.sln` i Visual Studio.
3.  **Start med Docker:** Velg `Docker` som kj�remilj� (i nedtrekksmenyen ved siden av "Start"-knappen) og trykk **Kj�r** (den gr�nne play-knappen, det skal st� **Container (Dockerfile)** ved siden av).
4.  **Resultat:** Applikasjonen blir tilgjengelig i nettleseren.

---

## 2. Systemarkitektur og Sentrale Komponenter

Applikasjonen f�lger et **ASP.NET Core MVC**-m�nster med en moderne, reaktiv frontend.

### A. Backend og Dataflyt
| Komponent | Form�l | Sentrale funksjoner |
| :--- | :--- | :--- |
| **`HomeController.cs` (Controller)** | Standard Navigasjon/Routing. | Fungerer som en ren ruter for `Index` og `Privacy`. All applikasjonslogikk ligger i `ObstacleController`. |
| **`ObstacleData.cs` (Model)** | Datamodell for hinderet. | Definerer og validerer data. Inkluderer GeoJSON-feltet (`ObstacleGeometry`) for lagring av kartkoordinater. |
| **`ObstacleController.cs` (Controller)** | H�ndterer applikasjonsflyten. | H�ndterer `[HttpGet]` (vis skjema) og `[HttpPost]` (validerer og overf�rer GeoJSON-data til oversiktssiden). |

### B. Frontend og Styling
| Komponent | Rolle | Sentrale funksjoner |
| :--- | :--- | :--- |
| **Tailwind CSS** | Styling og Responsivitet. | Alle klasser kompileres fra kildekoden via `npm run dev` (som kj�res i Docker). |
| **Leaflet** | Kartmotor. | Hovedbiblioteket for � vise kartet. |
| **Leaflet.draw** | Tegneverkt�y. | Tillater brukeren � tegne **punkter** og **linjer**. |

---

## 3. Testscenarier og Resultater

F�lgende tester bekrefter at applikasjonen oppfyller kravene til datafangst, dataintegritet, og visuell representasjon:

| ID | Testscenario | Handling | Forventet Resultat | Resultat |
| :--- | :--- | :--- | :--- | :--- |
| **T.1** | **Dataintegritet (Validering)** | Fors�k � sende inn et tomt skjema. | Skjemaet avvises. Stor, r�d feilmelding vises under hvert felt, inkludert kartet. | **Suksess** |
| **T.2** | **GeoJSON Punkt-flyt** | Tegn kun �n mark�r p� kartet og send inn. | Mark�ren vises p� oversikten. Statisk koordinatvisning viser n�yaktige Lat/Lng-koordinater. | **Suksess** |
| **T.3** | **GeoJSON Linje-flyt** | Tegn en linje med tre punkter. | Linjen vises p� oversikten. To permanente labels (**"Start Point"** og **"End Point"**) vises for referanse. | **Suksess** |
| **T.4** | **Responsivitet** | Endre nettleservinduet til mobil-/nettbrettst�rrelse. | Layouten til skjema og navigasjonsmenyen justeres korrekt (Tailwind CSS). | **Suksess** |
| **T.5** | **Arbeidsflyt** | G� fra startside -> Skjema -> Oversikt -> Tilbake til startside. | Full navigasjonssyklus fungerer, og dataene persisteres gjennom POST/GET-syklusen. | **Suksess** |

###### CHANGELOG FOR MATS BRANCH ######

Endringslogg - Backend-implementering med Entity Framework
🎯 Oversikt
Implementert en komplett ASP.NET Core MVC backend med Entity Framework Core for hinderregistrering. Applikasjonen støtter nå full CRUD-funksjonalitet med MariaDB database-integrasjon.

✨ Funksjoner som er lagt til
1. Entity Framework Core Oppsett

Installert EF Core 8.0.11 med Pomelo MySQL provider for MariaDB-støtte
Implementert Code-First tilnærming for database-generering
Opprettet migrasjonsfiler for database-skjema

2. Database-arkitektur

Opprettet Data/ApplicationDbContext.cs

DbContext for å håndtere database-operasjoner
Konfigurert Obstacles-tabell med unike begrensninger
Satt opp standardverdier og indekser


3. Forbedret datamodell

Oppdatert Models/ObstacleData.cs

Lagt til primærnøkkel (Id) med auto-increment
Lagt til RegisteredDate tidsstempel (auto-generert)
Lagt til valgfritt RegisteredBy felt for brukersporing
Lagt til valgfritt ObstacleType felt for kategorisering
Implementert datavaliderings-attributter
Lagt til visningsnavn for bedre brukergrensesnitt


4. Controller-forbedringer

Oppdatert Controllers/ObstacleController.cs

Implementert Dependency Injection for DbContext
Lagt til asynkrone database-operasjoner for bedre ytelse
Nye metoder:

DataForm (POST) - Lagrer hindre til databasen
Overview (GET) - Viser hinderdetaljer med kart
Index (GET) - Lister alle registrerte hindre


Implementert TempData for sporing av nye registreringer


5. Views opprettet/oppdatert

Views/Obstacle/Overview.cshtml

Forbedret til å fungere som både bekreftelsesside og detaljvisning
Viser hinder-ID, type og registrert av-informasjon
Viser registreringstidspunkt
Dynamiske meldinger basert på kontekst (ny vs eksisterende)
Interaktivt kart med GeoJSON-visualisering


Views/Obstacle/Index.cshtml (NY)

Tabellvisning av alle registrerte hindre
Sorterbare kolonner: ID, Navn, Høyde, Type, Dato, Registrert av
"View Details"-lenke for hvert hinder
Tom tilstand-melding når ingen hindre eksisterer
Hurtigknapper for registrering


6. Konfigurasjon

Oppdatert Program.cs

Registrert DbContext med Dependency Injection
Konfigurert MariaDB-tilkobling med versjon 10.11
Satt opp connection string-håndtering


🗄️ Database-skjema
Obstacles-tabell:
KolonneTypeBegrensningerIdINTPRIMARY KEY, AUTO_INCREMENTObstacleNameVARCHAR(100)NOT NULL, UNIQUEObstacleHeightDOUBLENOT NULL, 0-200ObstacleTypeVARCHAR(50)NULLABLEObstacleDescriptionVARCHAR(1000)NOT NULLObstacleGeometryTEXTNOT NULL (GeoJSON)RegisteredDateDATETIMEDEFAULT CURRENT_TIMESTAMPRegisteredByVARCHAR(100)NULLABLE

📦 NuGet-pakker som er lagt til
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.11" />

🔧 Tekniske detaljer
Arkitekturmønster: MVC (Model-View-Controller)
ORM: Entity Framework Core 8.0
Database: MariaDB 10.11
Migrasjonsstrategi: Code-First
Asynkront mønster: Fullstendig async/await implementering

🚀 Neste steg (Klare for implementering)

 Sett opp MariaDB database-tilkobling
 Kjør migrasjoner: dotnet ef database update
 Implementer Edit-funksjonalitet
 Implementer Delete-funksjonalitet
 Legg til søk/filter på Index-siden
 Implementer brukerautentisering med Identity
 Legg til bildeopplasting for hindre
 Forbedret validering og feilhåndtering


📝 Notater

Alle database-operasjoner bruker asynkrone metoder for optimal ytelse
Connection string bør lagres i User Secrets av sikkerhetshensyn
Migrasjoner er klare, men ikke kjørt ennå (venter på database-oppsett)
Koden inneholder norske kommentarer for teamforståelse


👥 Læringspunkter for teamet
Dependency Injection: Hvordan ASP.NET Core injiserer tjenester inn i kontrollere

Vi deklarerer ApplicationDbContext i konstruktøren
ASP.NET Core gir oss automatisk riktig instans
Dette gjør koden testbar og løst koblet

Async/Await: Hvorfor og hvordan bruke asynkrone database-operasjoner

async Task<IActionResult> lar metoden kjøre asynkront
await _context.SaveChangesAsync() venter på at operasjonen fullføres
Dette forhindrer at serveren blokkerer mens den venter på databasen
Bedre ytelse når mange brukere bruker appen samtidig

Data Annotations: Bruk av attributter for validering og database-konfigurasjon

[Required] - Feltet må fylles ut
[MaxLength] - Begrenser lengden på tekst
[Range] - Validerer at tall er innenfor et område
[Key] - Definerer primærnøkkel

Code-First Migrations: Generering av database-skjema fra C#-modeller

Vi skriver modeller i C# (f.eks. ObstacleData.cs)
Entity Framework leser modellene og lager SQL-kommandoer
Add-Migration lager en migrasjonsfil
Update-Database kjører SQL mot databasen

ViewBag vs TempData: Når man skal bruke hver for dataoverføring mellom actions

ViewBag: Kun innenfor samme request (Controller → View)
TempData: Overlever redirects (én gang)
TempData brukes når vi gjør RedirectToAction()


🎓 Viktige konsepter forklart
DbContext - Hva er det?

"Broen" mellom C#-kode og database
Holder styr på alle endringer du gjør
SaveChangesAsync() skriver alle endringer til databasen på én gang
Håndterer transactions automatisk

DbSet - Hva er det?

Representerer en tabell i databasen
DbSet<ObstacleData> Obstacles = Obstacles-tabellen
Lar oss kjøre LINQ-spørringer mot tabellen

Migration - Hva er det?

En "oppskrift" for hvordan databasen skal endres
Inneholder Up() (oppdater database) og Down() (angre endring)
Versionskontroll for database-skjemaet
Kan kjøres på alle miljøer (Development, Production)