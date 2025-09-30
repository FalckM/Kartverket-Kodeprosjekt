# Webapplikasjon for registrering av luftfartshindre

## 1. Driftsdokumentasjon: Slik kj�rer du applikasjonen

Prosjektet er satt opp for kj�ring via **Docker** for � sikre et portabelt milj�. Denne metoden sikrer at Node.js, .NET SDK, og alle avhengigheter er riktig konfigurert og tilgjengelige i containeren.

**N�dvendige verkt�y:**

* **Docker Desktop** (m� kj�re i bakgrunnen)
* **Visual Studio** (Med Container Development Workloads)

**Stegvis kj�ring:**

1.  **Klone Prosjektet:** Klone dette Git-repositoriet.
2.  **�pne L�sningen:** �pne `FirstWebApplication.sln` i Visual Studio.
3.  **Start med Docker:** Velg `Docker` som kj�remilj� (i nedtrekksmenyen ved siden av "Start"-knappen) og trykk **Kj�r** (den gr�nne play-knappen, det skal st� **Container (Dockerfile)** ved siden av).
4.  **Resultat:** Applikasjonen blir tilgjengelig i nettleseren.

---

## 2. Systemarkitektur og Sentrale Komponenter

Applikasjonen f�lger et **ASP.NET Core MVC**-m�nster med en moderne, reaktiv frontend.

### A. Backend og Dataflyt
| Komponent | Form�l | Sentrale funksjoner |
| :--- | :--- | :--- |
| **`ObstacleData.cs` (Model)** | Datamodell for hinderet. | Inkluderer GeoJSON-feltet (`ObstacleGeometry`) for lagring av kartkoordinater. |
| **`ObstacleController.cs` (Controller)** | H�ndterer `[HttpGet]` for skjemaet og `[HttpPost]` for � validere og overf�re data til oversiktssiden. | Fjerner all MariaDB-logikk for � kun fokusere p� oppgavens omfang. |

### B. Frontend og Styling
| Komponent | Rolle | N�kkelpunkter |
| :--- | :--- | :--- |
| **Tailwind CSS** | Styling og Responsivitet. | Alle klasser kompileres fra kildekoden via `npm run dev` (som kj�res i Docker). |
| **Leaflet** | Kartmotor. | Hovedbiblioteket for � vise OpenStreetMap-kartet. |
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
| **T.5** | **Arbeidsflyt (Oppgave 4)** | G� fra startside ? Skjema ? Oversikt ? Tilbake til startside. | Full navigasjonssyklus fungerer, og dataene persisteres gjennom POST/GET-syklusen. | **Suksess** |