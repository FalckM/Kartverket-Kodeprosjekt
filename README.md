# Webapplikasjon for registrering av luftfartshindre

## 1. Driftsdokumentasjon: Slik kjører du applikasjonen

Prosjektet er satt opp for kjøring via **Docker** for å sikre et portabelt miljø. Denne metoden sikrer at Node.js, .NET SDK, og alle avhengigheter er riktig konfigurert og tilgjengelige i containeren.

**Nødvendige verktøy:**

* **Docker Desktop** (må kjøre i bakgrunnen)
* **Visual Studio** (Med Container Development Workloads)

**Stegvis kjøring:**

1.  **Klone Prosjektet:** Klone dette Git-repositoriet.
2.  **Åpne Løsningen:** Åpne `FirstWebApplication.sln` i Visual Studio.
3.  **Start med Docker:** Velg `Docker` som kjøremiljø (i nedtrekksmenyen ved siden av "Start"-knappen) og trykk **Kjør** (den grønne play-knappen, det skal stå **Container (Dockerfile)** ved siden av).
4.  **Resultat:** Applikasjonen blir tilgjengelig i nettleseren.

---

## 2. Systemarkitektur og Sentrale Komponenter

Applikasjonen følger et **ASP.NET Core MVC**-mønster med en moderne, reaktiv frontend.

### A. Backend og Dataflyt
| Komponent | Formål | Sentrale funksjoner |
| :--- | :--- | :--- |
| **`ObstacleData.cs` (Model)** | Datamodell for hinderet. | Inkluderer GeoJSON-feltet (`ObstacleGeometry`) for lagring av kartkoordinater. |
| **`ObstacleController.cs` (Controller)** | Håndterer `[HttpGet]` for skjemaet og `[HttpPost]` for å validere og overføre data til oversiktssiden. | Fjerner all MariaDB-logikk for å kun fokusere på oppgavens omfang. |

### B. Frontend og Styling
| Komponent | Rolle | Nøkkelpunkter |
| :--- | :--- | :--- |
| **Tailwind CSS** | Styling og Responsivitet. | Alle klasser kompileres fra kildekoden via `npm run dev` (som kjøres i Docker). |
| **Leaflet** | Kartmotor. | Hovedbiblioteket for å vise OpenStreetMap-kartet. |
| **Leaflet.draw** | Tegneverktøy. | Tillater brukeren å tegne **punkter** og **linjer**. |

---

## 3. Testscenarier og Resultater

Følgende tester bekrefter at applikasjonen oppfyller kravene til datafangst, dataintegritet, og visuell representasjon:

| ID | Testscenario | Handling | Forventet Resultat | Resultat |
| :--- | :--- | :--- | :--- | :--- |
| **T.1** | **Dataintegritet (Validering)** | Forsøk å sende inn et tomt skjema. | Skjemaet avvises. Stor, rød feilmelding vises under hvert felt, inkludert kartet. | **Suksess** |
| **T.2** | **GeoJSON Punkt-flyt** | Tegn kun én markør på kartet og send inn. | Markøren vises på oversikten. Statisk koordinatvisning viser nøyaktige Lat/Lng-koordinater. | **Suksess** |
| **T.3** | **GeoJSON Linje-flyt** | Tegn en linje med tre punkter. | Linjen vises på oversikten. To permanente labels (**"Start Point"** og **"End Point"**) vises for referanse. | **Suksess** |
| **T.4** | **Responsivitet** | Endre nettleservinduet til mobil-/nettbrettstørrelse. | Layouten til skjema og navigasjonsmenyen justeres korrekt (Tailwind CSS). | **Suksess** |
| **T.5** | **Arbeidsflyt (Oppgave 4)** | Gå fra startside ? Skjema ? Oversikt ? Tilbake til startside. | Full navigasjonssyklus fungerer, og dataene persisteres gjennom POST/GET-syklusen. | **Suksess** |