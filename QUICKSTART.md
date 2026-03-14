# Legal Document Comparator — Quick Start

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 8.0+ | [Download](https://dotnet.microsoft.com/download) |
| Node.js | 20+ | [Download](https://nodejs.org) |

---

## 1. Configure the Groq API Key

The AI comparison feature requires a Groq API key.

1. Get a free key at **https://console.groq.com/keys**
2. Open `backend/src/LegalDocumentComparator.WebApi/appsettings.json` and replace `your-groq-api-key-here` with your actual key:
   ```json
   "AI": {
     "Groq": {
       "ApiKey": "gsk_your_actual_key_here"
     }
   }
   ```
OR write me in Teams. I'll give you a key.
---

## 2. Start the Backend

```bash
cd backend/src/LegalDocumentComparator.WebApi
dotnet run
```

The API will be available at **http://localhost:5000**.  
On first run it:
- Creates a local SQLite database at `data/legal-comparator.db`
- Seeds two default accounts (see below)

---

## 3. Start the Frontend

Open a second terminal:

```bash
cd frontend
npm install        # first time only
npm run dev
```

Opens at **http://localhost:5173**

> The frontend calls `http://localhost:5000/api/v1` by default.  
> To use a different backend URL, copy `.env.example` to `.env.local` and edit `VITE_API_BASE_URL`.

---

## Default Accounts

| Role    | Email              | Password   |
|---------|--------------------|------------|
| User    | user@legal.com     | User123!   |
| Manager | admin@legal.com    | Admin123!  |

---

## Workflow

### Upload & Compare (5 steps)

1. **Sign in** at http://localhost:5173 → you land on the **Document Library**.
2. Click **+ Upload Document** → fill in name, version number, pick a PDF → **Upload**. Use Sample PDF files from "pdf documents" folder.
3. Expand the document card → click **+ Add Version** → upload a second PDF version.
4. **Tick** two version checkboxes (same document) → a **Compare** bar appears at the bottom.
5. Click **Compare** → the AI analyzes changes (2–60 sec depending on provider).

The full-page **Comparison View** opens with:
- Side-by-side PDF panels with color-coded change highlights
- A changes sidebar (filter by severity, click to jump to page)
- A collapsible AI Analysis panel (summary, legal implications, risk, key changes)

### Regenerate

Click **Regenerate** in the Comparison View header → confirm the dialog → a fresh AI analysis runs.

### Manager Dashboard

Log in as `admin@legal.com` → use the **Manager Dashboard** link in the header to see all users' documents.
