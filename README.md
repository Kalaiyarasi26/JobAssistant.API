# Job Application Assistant API

Backend service that compares resume content with a job description and returns AI-generated career guidance. The application is built with ASP.NET Core on .NET 10.

## Architecture

The API keeps transport, document processing, and AI orchestration in separate layers:

- **AnalyzeController** handles HTTP input validation and coordinates the analysis workflow.
- **PdfService** extracts text from uploaded PDFs using PdfPig. The controller passes that text into the same analysis path used for text-based resumes.
- **ClaudeService** builds the analysis prompt and calls `IChatClient` from `Microsoft.Extensions.AI`.
- **Program.cs** is the composition root: it configures dependency injection and adapts the Anthropic SDK to `IChatClient`.
- **Models** define the request and response contracts.

The `IClaudeService` and `IPdfService` interfaces isolate controller behavior from provider and parsing implementations. This keeps those boundaries independently testable and leaves the AI provider adapter replaceable without coupling the controller to the Anthropic SDK. Cancellation tokens flow from the HTTP request through the service calls.

## Verification

The test project covers service behavior, controller validation, PDF extraction, and HTTP integration through `WebApplicationFactory<Program>`. Integration tests replace `IChatClient` with a deterministic fake, so they exercise the ASP.NET Core pipeline without credentials or external provider calls. Test sources are excluded from the API project's build and publish inputs.

## Work Completed

- Established controller, PDF-processing, and AI-service boundaries using dependency injection and interfaces.
- Added unit and in-memory integration coverage for the analysis workflows.
- Isolated integration tests from the external AI provider with a fake chat client.
- Separated test artifacts from API build/publish inputs and updated the Swagger dependency to address a reported package advisory.
