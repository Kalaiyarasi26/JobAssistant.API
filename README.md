# Job Assistant 🤖

An AI-powered job application assistant built with C# .NET and the official Anthropic Claude SDK.

## What it does
Paste a job description and your resume — get instant gap analysis, tailored resume bullets, and interview prep questions powered by Claude AI.

## Tech Stack
- **Backend:** C# ASP.NET Core Web API
- **AI:** Anthropic Claude API (official C# SDK)
- **Frontend:** React (in progress)

## How it works
1. User inputs their resume and a job description
2. Backend chunks and sends relevant content to Claude via the official Anthropic C# SDK
3. Claude returns gap analysis, tailored bullets, and interview questions
4. React frontend displays the results

## Getting Started

### Prerequisites
- .NET 10
- Node.js
- Anthropic API key from console.anthropic.com

### Backend Setup
```bash
cd JobAssistant.API
dotnet user-secrets set "ANTHROPIC_API_KEY" "your-key-here"
dotnet run
```
## Status
🚧 In progress — Day 1 complete, Claude API connected and responding.
