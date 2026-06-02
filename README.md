# Job Application Assistant - Backend API

This is the backend API for the Job Application Assistant, a career coaching platform powered by Claude AI.

## Tech Stack

- **Runtime**: C# .NET 10
- **Framework**: ASP.NET Core
- **AI**: Official Anthropic SDK with IChatClient from Microsoft.Extensions.AI
- **PDF Processing**: PdfPig
- **API Documentation**: Swagger/OpenAPI

## Features

- Career gap analysis powered by Claude AI
- PDF resume extraction and processing
- Tailored resume bullet point suggestions
- Interview question generation
- RESTful API endpoints

## Endpoints

### 1. POST /api/analyze

Analyzes a resume against a job description using JSON input.

**Request Body:**
```json
{
  "resumeText": "string",
  "jobDescription": "string"
}
```

**Response:**
```json
{
  "advice": "string (markdown formatted)"
}
```

**Returns:** Gap analysis, strengths, tailored resume bullets, and interview preparation tips.

### 2. POST /api/analyze/upload

Analyzes a resume against a job description using a PDF file upload.

**Request:**
- Content-Type: `multipart/form-data`
- Fields:
  - `resumePdf`: PDF file (required)
  - `jobDescription`: text (required)

**Response:**
```json
{
  "advice": "string (markdown formatted)"
}
```

**Returns:** Gap analysis, strengths, tailored resume bullets, and interview preparation tips.

## Setup Instructions

### Prerequisites

- .NET 10 SDK installed
- Git installed
- Anthropic API key (get one at [console.anthropic.com](https://console.anthropic.com))

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repo-url>
   cd job-assistant/JobAssistant.API
   ```

2. **Set the Anthropic API key:**
   ```bash
   dotnet user-secrets set ANTHROPIC_API_KEY your-key-here
   ```

3. **Run the API:**
   ```bash
   dotnet run
   ```

The API will start at `http://localhost:5149` and automatically open Swagger UI for testing.

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:3000`
- `https://localhost:3000`

This allows seamless integration with React apps running on the default localhost:3000 port.

## API Documentation

Once the API is running, visit:
- Swagger UI: `http://localhost:5149/swagger`
- OpenAPI spec: `http://localhost:5149/swagger/v1/swagger.json`

Use Swagger UI to test endpoints directly in the browser.

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet run
```

Then use Swagger UI or tools like Postman/cURL to test the endpoints.

## Architecture

- **ClaudeService**: Handles AI analysis via Claude with IChatClient abstraction
- **PdfService**: Extracts text from uploaded PDF files
- **AnalyzeController**: HTTP endpoints for analysis requests
- **Program.cs**: Dependency injection setup, CORS configuration, middleware pipeline

## Notes

- The service uses IChatClient abstraction, making it possible to swap AI providers in the future
- All responses include markdown-formatted advice suitable for rich text display
- PDF extraction preserves page breaks for better text organization
- Maximum output tokens per request: 1024
