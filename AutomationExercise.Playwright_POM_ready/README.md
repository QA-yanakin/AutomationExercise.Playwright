# AutomationExercise.Playwright (C# + NUnit + Playwright)

## Run
1) Open in Visual Studio 2022+
2) `pwsh ./playwright.ps1 install`
3) Set `DefaultUser` in `appsettings.json` (or run TC01 to create user)
4) Run tests from Test Explorer.

## Structure
- Pages/ (POM)
- Components/
- Support/ (TestBase, TestSettings)
- Tests/ (NUnit fixtures)

Selectors use ARIA roles/placeholders where stable; fallback to CSS/ID; avoid brittle XPaths.
