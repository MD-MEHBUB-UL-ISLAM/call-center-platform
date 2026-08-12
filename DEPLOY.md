# Deploying this monorepo

Repo layout:
```
call-center-platform/
├── frontend/   Angular app  → Vercel
└── backend/    .NET 10 API  → Railway
```

Both platforms support **monorepos** by letting you set a "root directory" so each
one only builds its own folder. Push this whole repo to GitHub first, then connect
each service.

## 1. Push to GitHub

```bash
cd call-center-platform
git init
git add .
git commit -m "Initial monorepo"
git branch -M main
git remote add origin https://github.com/<you>/call-center-platform.git
git push -u origin main
```

## 2. Backend → Railway (deploy this first)

1. In Railway: **New Project → Deploy from GitHub repo** → select this repo.
2. Open the service's **Settings → Root Directory** and set it to `backend`.
   Railway will find `backend/railway.toml`, which points it at `backend/Dockerfile`
   (a Dockerfile is used instead of Nixpacks since .NET 10 support there is spotty).
3. Under **Variables**, add:
   | Variable | Value |
   |---|---|
   | `Jwt__Key` | a long random secret (32+ chars) — do **not** reuse the dev key in `appsettings.json` |
   | `Jwt__Issuer` | `CallCenterPrototype` (or your own) |
   | `Jwt__Audience` | `CallCenterPrototypeClient` |
   | `Cors__AllowedOrigins__0` | your Vercel URL, e.g. `https://call-center-platform.vercel.app` |
   | `ConnectionStrings__Default` | see the SQLite note below |

   Railway auto-injects `PORT`; `Program.cs` already reads it and binds Kestrel to
   `0.0.0.0:$PORT`, so you don't need to set that yourself.
4. Deploy. Railway gives you a public URL like
   `https://call-center-backend-production.up.railway.app` — copy it, you'll need
   it for the frontend.
5. **SQLite note:** Railway's default filesystem is ephemeral — `callcenter.db`
   gets wiped on every redeploy. For anything beyond a demo, add a
   [Railway Volume](https://docs.railway.app/guides/volumes) (e.g. mounted at
   `/data`) and set `ConnectionStrings__Default` to
   `Data Source=/data/callcenter.db`. Swapping to Postgres is the more
   production-appropriate fix if you outgrow this.

## 3. Frontend → Vercel

1. In Vercel: **Add New → Project** → import the same GitHub repo.
2. Set **Root Directory** to `frontend`. Vercel will pick up `frontend/vercel.json`
   for the build output path and the SPA rewrite (needed so Angular's client-side
   routing doesn't 404 on refresh).
3. Before deploying (or right after, then redeploy), update
   `frontend/src/environments/environment.prod.ts` with the real Railway URL from
   step 2 above:
   ```ts
   export const environment = {
     production: true,
     apiBaseUrl: 'https://<your-railway-app>.up.railway.app/api',
     signalRHubUrl: 'https://<your-railway-app>.up.railway.app/hubs/call'
   };
   ```
   Commit and push — Vercel redeploys automatically on push to `main`.
4. Deploy. Vercel gives you a URL like `https://call-center-platform.vercel.app`.

## 4. Close the loop

Go back to Railway and make sure `Cors__AllowedOrigins__0` matches the **exact**
Vercel URL from step 3 (including `https://`, no trailing slash) — the API's CORS
policy will reject the browser otherwise. Redeploy the backend if you changed it.

## 5. Sanity check

- Open the Vercel URL, log in, confirm API calls succeed (Network tab should hit
  the Railway domain with 2xx responses).
- Confirm the SignalR connection in the browser console shows `Connected` rather
  than looping reconnect attempts — that usually means CORS or the hub URL is
  still wrong.
