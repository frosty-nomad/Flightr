# Flightr

Flightr helps pilots track flight log entries and progress toward a pilot license. The solution contains a Razor Pages front end, a C# Web API middle tier, and a shared data library with EF Core models.

## Vision statement

For private and commercial pilots who want a clear, trustworthy record of their training progress, Flightr is a simple web app that lets you log the FAA-required basics quickly after each flight. Unlike spreadsheets or generic logbook apps, Flightr keeps the focus on staying current, tracking progress, and preparing for checkrides, with an easy-to-use interface and reliable back-end storage. Use it to track the time and experience you need to earn your private or commercial pilot's license.

## Features

- Account registration with pilot profile (name, license number, expiration date) and license goal selection.
- Secure login/logout with persistent sessions.
- Flight log entry creation, editing, and deletion with FAA minimum required fields.
- Protected "My Flight Logs" view with summary table.
- CSV download endpoint for flight logs.
