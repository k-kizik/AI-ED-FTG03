# Legal Document Comparator

A web application for comparing versions of legal PDF documents using AI. 

Upload multiple versions of a contract, agreement, or any legal document, then compare any two versions side-by-side. The app highlights text additions, deletions, and modifications directly on the PDF pages, and generates an AI-powered analysis explaining the legal implications of each change, assessing risk, and summarizing what has been altered between versions.

## Key Features

- **Document Library** — upload PDFs and organize them by document with version history
- **Side-by-side comparison** — two PDF panels with color-coded diff highlights per page
- **AI analysis** — legal summary, risk assessment, and per-change legal meaning (powered by Groq Cloud)
- **Role-based access** — regular users manage their own documents; managers see all users' documents
- **Comparison caching** — results are saved and reused; regeneration available on demand

## Stack

- **Backend** — ASP.NET Core 8, Clean Architecture, SQLite, EF Core, PdfPig, DiffPlex, JWT
- **Frontend** — React 18, TypeScript, Vite, Tailwind CSS, TanStack Query, react-pdf

See [QUICKSTART.md](QUICKSTART.md) to get up and running in minutes.

## Project Structure

```
AI-ED-FTG03/
├── pdf documents/    ← Sample PDF files for testing uploads and comparisons
├── backend/          ← ASP.NET Core 8 Web API (C#)
│   └── src/
│       ├── LegalDocumentComparator.WebApi/        ← HTTP layer, controllers
│       ├── LegalDocumentComparator.Application/   ← Use cases, business logic
│       ├── LegalDocumentComparator.Domain/        ← Entities, value objects
│       └── LegalDocumentComparator.Infrastructure/← EF Core, PDF, AI services
└── frontend/         ← React 18 + TypeScript + Vite
    └── src/
        ├── api/          ← Axios API clients
        ├── components/   ← Reusable UI components
        │   ├── layout/
        │   ├── library/  ← DocumentCard, VersionRow, UploadModal, CompareBar
        │   ├── comparison/  ← PdfPanel, ChangesSidebar, AnalysisPanel
        │   └── shared/   ← SeverityBadge, LoadingOverlay, ProtectedRoute
        ├── context/      ← AuthContext (JWT, user, role)
        ├── hooks/        ← useDocuments, useVersionSelection
        ├── pages/        ← LoginPage, RegisterPage, LibraryPage, ComparisonPage
        └── types/        ← TypeScript interfaces matching backend DTOs
```

# How it was built

Almost all the code, except this document, was built using GitHub Copilot. The backend was made by Copilot in .Net Visual Studio. For the frontend, Copilot from VS Code was involved.
No MCP servers or external tools.

The model - Claude Sonnet 4.6.

## Workflow

### Backend

First I described the whole task in the ask mode.

>Need a Legal Document Comparator application.
A tool for lawyers: it reads new versions of legal documents, compares them with old ones, and sends a summary with key changes and their meaning.
Requirements: 
A web interface and a backend.
AI is used for comparing the documents. Local library is better than cloud AI service.
It should be easily deployable on a local computer. No cloud services.
A local database storage.
All the libraries are free or open source.
Clean architecture.
What languages, frameworks are better for the task?
Ask questions if needed?

Then I worked with Copilot answering its questions.

>Answers to the questions:
>1. PDF format.
>2. Up to 100 pages documents, but shouldn't be an issue to process a slightly larger document.
>3. AI should identify changes, highlight the different places in compared versions of a document. Semantic understanding of legal changes is needed. AI should explain the legal implications of changes.
>4. Multiple users with a simple authentication. Two roles: a user and a manager. A user can manage only their uploaded documents. Manager can manage all document in the app. You decide if concurrent document comparisons needed.
>5. A comparison should be real-time for now and produced only upon request. The result of a comparison should be saved to not to generate it again. There should be a posibility to regenerate a comparision result.
>6. Deployment Target is local windows computer. The architecture should  provide for the possibility of deployment on Linux in the future.
Easy local running of the application is required.
If I use dotnet instead of python, will it make it more difficult to find libraries?

>Use PdfPig, Microsoft Semantic Kernel. Can we use SQLite?
>What web app routing structure is better for the task? Ask questions if needed?

Then I switched Copilot to Agent mode

>The structure looks good. Generate 1.	Complete Backend first. Ask questions if needed.

I answered the Agent's questions

>Answers to Backend Configuration Questions:
>1.	OK for both.
>2.	OK for both.
>3.	good idea.
>4.	OK
>5.	simple JWT token
>6.	Use versioning
>7.	option B.
>8.	structured error responses.

>Add Groq as a primary service. Keep ability to switch to the local llama (optional service).

When the most of backend was ready, I started refactoring. I still haven't written a line of code.

>Should we keep copies of the prompts in the providers or it is better to have the prompts and logic in a single place (AiService) and use the providers just for the communication?
(argumented answer was - "no")

>Let's rewrite logic to save the data to the DB. Remove logic from entities. Rewrite repositories if needed. If we need to add a new document version, do it explixitly. Ask questions if needed.

### Frontend

I switched to VS Code, because Copilot in .Net Visual Studio started to freeze. I'd do the entire frontend in VS if it weren't for this problem. 

Ask mode (I didn't use Plan).

>Let's make a plan for adding the UI for LegalDocumentComparator.
Requirements:
>1. single-page workflow.
>2. Should be a page for library/list view. A list of documents. A document has a list of versions. CRUD operation are awailable for each item.
>3. A separate dashboard for managers. The only difference is that managers can see all the documents that belong to any user.
>4. A full-page view.
No need to show saved comparisons. When a user selects two versions of a document, "compare" button should appear. If a comparison is saved, just load it. If no comparisons were found, generate a new one. There should be option to regenerate a comparision for any pair of a document version.
Ask questions if needed.

Copilot asks,
I answer:

> - Q: What UI technology stack should be used?
> - A: React + TypeScript
> - Q: What should the "full-page comparison view" show?
> - A: All of the above combined
> - Q: The backend is currently missing: delete document, delete version, update document name/description. Should we plan to add these endpoints too?
> - A: No, keep backend as-is (omit delete/update from UI)
> - Q: "Single-page workflow" — do you mean a SPA (no full page reloads, client-side routing) or literally everything on one scrollable page with no routing?
> - A: Just use a common React approach
> - Q: When selecting two versions to compare, where should the selection happen?
> - A: In the document list view — checkboxes on version rows

> Key Constraints / Edge Cases should be handled.
Add GET /api/v1/documents/versions/ endpoint on the backend side to load a document text.
The frontend architecture should be clear and simple.
It should be easily launched on a local computer.
Add a quick start  document.

Copilot was switched to Agent mod and initial version was created.
Then a small manual test and refactoring.

> Is it possible to highlight the text differences with color in the "Original" and "New version" panels? Do it in the frontend app only. Ask questions if needed.

> Remove Ollama provider so only Groq is used.

> Cover backend and frontend with unit tests. ask questions if needed

## Conclusions

My main way of working was: 

1. Description of the entire task in Ask mode.
2. Answering Copilot's questions.
3. Adding additional requirements, clarifications.
4. Switching to the Agent mode and implementing everything according the plan.
5. Manual testing.
6. Refactoring using commands to Copilot.

In the examples above, I’ve highlighted the main prompts, leaving out the routine ones.

I’m used to using the code assistant in .Net VS and find it just as productive as in VS Code.
Unfortunately, however, there are a few issues with Visual Studio that I have noted.
Sometimes the Copilot just freezes, and once it starts, it freezes on every task. I don’t know why.
I’ve also had a bad experience integrating an MCP server into .Net Visual Studio on my work project.

There was also a situation where, during a manual test, I discovered an issue relating to a database entry saving. Neither in VS nor in VS Code Copilot was not able to identify the cause and fix the code.
I had to rewrite the repositories and the logic in a different way (using prompts, of course).