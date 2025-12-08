# HeadsUp - Project Status & Context

## Project Overview
HeadsUp ist ein mobiles Wortspiel (ähnlich wie Hedbanz/Stirnraten) für Android/iOS. Der Spieler hält das Handy an die Stirn und andere Spieler geben Hinweise. Durch Kippen nach unten = richtig, nach oben = überspringen.

## Technische Details
- **Unity Version**: 2022.3 LTS
- **Plattform**: Android (primär), iOS (später)
- **Orientierung**: Portrait 9:16 (1080x1920), locked
- **Input System**: New Unity Input System (nicht das alte!)
- **UI**: TextMeshPro, Canvas mit CanvasScaler
- **Sprache**: Deutsch (Lokalisierung wurde entfernt für bare bones Version)

## Aktuelle Projektstruktur

### Scripts Organisation
```
Assets/Scripts/
├── Managers/
│   ├── GameManager.cs          # Core Spiellogik, Singleton
│   └── UIManager.cs            # Screen Management, UI Updates
├── Controllers/
│   ├── GameplayController.cs   # Gameplay Input, Timer, Accelerometer
│   ├── ReadyScreenController.cs # Tilt-to-Start Detection
│   └── CategoryLoader.cs       # JSON Kategorien laden
├── Data/
│   ├── Category.cs             # ScriptableObject für Kategorien
│   └── Player.cs               # Player Datenklasse
└── Editor/
    ├── GameScreenSetup.cs      # Tool: UI Elemente für GameScreen erstellen
    └── InputSystemFixer.cs     # Tool: Input System Fix (StandaloneInputModule → InputSystemUIInputModule)
```

### Resources
```
Assets/Resources/Categories/
├── Animals.json                # 49 Wörter
└── Food.json                   # 58 Wörter (Food & Drinks)
```

### UI Struktur (Hierarchy)
```
HeadsUp/
├── GameManager (GameObject)
├── CategoryLoader (GameObject)
├── UIManager (GameObject)
├── General/
│   ├── EventSystem (mit InputSystemUIInputModule!)
│   └── Main Camera
└── UI/
    ├── MainMenuScreen (aktiv)
    ├── PlayerSetupScreen (inaktiv)
    ├── ReadyScreen (inaktiv)
    ├── GameScreen (inaktiv)
    │   ├── TimerText
    │   ├── WordText
    │   ├── PlayerNameText (fehlt noch!)
    │   └── HintText (fehlt noch!)
    ├── ResultsScreen (inaktiv)
    └── LeaderboardScreen (inaktiv)
```

## UI Flow
1. **MainMenuScreen**: Kategorie auswählen (Animals, Food, Random)
2. **PlayerSetupScreen**: Spieler hinzufügen/entfernen
3. **ReadyScreen**: Tilt down um zu starten
4. **GameScreen**: 60 Sekunden spielen (Tilt down = richtig, Tilt up = überspringen)
5. **ResultsScreen**: Score + farbige Wortliste (grün = richtig, rot = übersprungen)
6. **LeaderboardScreen**: Alle Spieler Scores

## Wichtige Features

### Spielmechanik
- **Rundenzeit**: 60 Sekunden (konfigurierbar in GameManager.roundDuration)
- **Tilt Controls**: 
  - Tilt down (threshold 0.5) = Wort richtig → grüner Flash
  - Tilt up (threshold 0.5) = Wort überspringen → roter Flash
  - Cooldown: 0.5s zwischen Tilts
- **Keyboard Fallback** (Editor testing):
  - Down Arrow / Space = richtig
  - Up Arrow = überspringen
- **Background Flashing**: Visual Feedback (grün/rot) bei Aktionen
- **Word Tracking**: Ergebnisse werden für Results Screen gespeichert

### Kategorien System
- JSON-basiert in `Resources/Categories/`
- CategoryLoader lädt automatisch alle JSON files
- Jede Kategorie hat: Name, Farbe, Wörter-Array
- Wörter werden pro Runde rotiert (keine Wiederholungen)

## Gelöste Probleme

### Input System Fix
**Problem**: `InvalidOperationException: You are trying to read Input using the UnityEngine.Input class`

**Lösung**: 
- EventSystem hatte `StandaloneInputModule` (altes Input System)
- Gelöst mit Tool: **Tools > HeadsUp > Fix Input System**
- Entfernt StandaloneInputModule, fügt InputSystemUIInputModule hinzu

### Quit Button entfernt
- QuitButton aus MainMenu entfernt (nicht nötig für Mobile)
- UIManager.QuitGame() Methode gelöscht
- quitButton Referenz entfernt

### DontDestroyOnLoad Warning
**Warnung**: "DontDestroyOnLoad only works for root GameObjects"
- GameManager ist kein Root GameObject (wahrscheinlich Child)
- **Kann ignoriert werden** - nur eine Warnung, keine Funktionsbeeinträchtigung

## Aktuelle Probleme & Nächste Schritte

### Problem: GameScreen UI Elemente fehlen
**Status**: Noch nicht gelöst!

**Symptom**:
```
NullReferenceException: Object reference not set to an instance of an object
GameplayController.ShowNextWord () (line 187)
GameplayController.UpdateTimerDisplay () (line 146)
```

**Ursache**: 
- GameScreen hat KEINE UI-Kinder für Word, Timer, PlayerName
- GameplayController kann Referenzen nicht finden
- Automatische FindUIReferences() findet nichts

**Lösung**:
1. Tool ausführen: **Tools > HeadsUp > Setup GameScreen UI**
   - Erstellt: WordText, TimerText, PlayerNameText, HintText
2. Danach im Inspector bei GameplayController Referenzen zuweisen:
   - Word Text → WordText
   - Timer Text → TimerText
   - Player Name Text → PlayerNameText
   - Game Screen → GameScreen GameObject

### Font Warnungen
**Warnung**: Unicode characters `\u2B07` (⬇️) und `\u2B06` (⬆️) nicht in LiberationSans SDF Font

**Lösung später**: 
- Andere Font verwenden die Emojis unterstützt
- Oder Emojis durch Text ersetzen ("DOWN" / "UP")

## Wichtige Code-Snippets

### GameplayController - Automatische Referenz-Suche
```csharp
private void FindUIReferences()
{
    if (gameScreen == null)
        gameScreen = GameObject.Find("GameScreen");
    
    if (gameScreen != null)
    {
        if (wordText == null)
            wordText = gameScreen.transform.Find("WordText")?.GetComponent<TextMeshProUGUI>();
        if (timerText == null)
            timerText = gameScreen.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        if (playerNameText == null)
            playerNameText = gameScreen.transform.Find("PlayerNameText")?.GetComponent<TextMeshProUGUI>();
    }
}
```

### UIManager - Referenzen die zugewiesen werden müssen
```csharp
[Header("Screen References")]
public GameObject mainMenuScreen;
public GameObject playerSetupScreen;
public GameObject readyScreen;
public GameObject gameScreen;
public GameObject resultsScreen;
public GameObject leaderboardScreen;

[Header("Main Menu - Category Buttons")]
public Button[] categoryButtons;  // Animals, Food Buttons
public Button randomCategoryButton;

[Header("Game Screen")]
public GameplayController gameplayController;
public Image gameBackgroundImage;
```

## Entfernte Features (für bare bones)
Diese Features wurden bewusst entfernt um erst die Kernfunktionalität zu haben:
- ❌ SoundManager
- ❌ VibrationManager
- ❌ LocalizationManager (war EN/DE)
- ❌ LocalizedText Komponente
- ❌ LanguageButton
- ❌ TextAnimator
- ❌ CategoryButton (eigene Komponente)
- ❌ CategoryEditor (ScriptableObject Editor)

**Plan**: Diese Features später wieder hinzufügen wenn Kernspiel funktioniert

## Development Workflow

### Bei Neustart
1. Unity Projekt öffnen
2. Szene öffnen (sollte HeadsUp.unity sein)
3. Play drücken und testen

### Bei Fehlern
1. Console öffnen (Ctrl+Shift+C)
2. Fehler analysieren
3. Häufig: Fehlende Referenzen im Inspector

### Tools im Menü
- **Tools > HeadsUp > Fix Input System**: EventSystem für neues Input System konfigurieren
- **Tools > HeadsUp > Setup GameScreen UI**: Fehlende UI Elemente für GameScreen erstellen

## Nächste Prioritäten
1. ✅ Input System Fix (erledigt)
2. 🔄 GameScreen UI Elemente erstellen (Setup Tool vorhanden, muss ausgeführt werden)
3. ⏳ GameplayController Referenzen im Inspector zuweisen
4. ⏳ Erste komplette Testrunde spielen
5. ⏳ Android Build testen
6. ⏳ Features hinzufügen (Sound, Vibration, etc.)

## Testing Checklist
- [ ] Kategorie auswählen funktioniert
- [ ] Spieler hinzufügen/entfernen funktioniert
- [ ] Ready Screen zeigt richtigen Spieler
- [ ] Tilt down startet Spiel
- [ ] Timer countdown funktioniert
- [ ] Tilt down = richtig (grüner Flash)
- [ ] Tilt up = überspringen (roter Flash)
- [ ] Results Screen zeigt Score + Wortliste
- [ ] Leaderboard zeigt alle Spieler
- [ ] Nächster Spieler wird korrekt gewechselt
- [ ] Accelerometer funktioniert auf Mobile

## Wichtige Notizen
- **IMMER** das neue Input System verwenden!
- GameScreen ist standardmäßig **inaktiv** in der Hierarchy
- Bare bones first - Features später hinzufügen
- Deutsche Texte verwenden (Lokalisierung kommt später)
- Portrait Mode locked für Mobile

## Kontakt & Fortsetzung
Wenn ein neuer Agent übernimmt:
1. Diese Datei lesen
2. Aktuellen Unity Console Output prüfen
3. Fehlende GameScreen UI Elemente als erstes beheben
4. Inspector Referenzen überprüfen
5. Dann weitermachen mit Testing
