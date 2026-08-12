# Deployment Strategy

## 1. Environments

| Environment | Purpose |
|---|---|
| **Dev** | Individual/team development, integrates with a sandbox/test SIP trunk and a CRM sandbox (or mocked CRM) |
| **Staging** | Mirrors production config; used for QA, load testing, and UAT before release |
| **Production** | Live agent traffic |

Each environment is a fully separate deployment (own database, own Redis, own telephony
trunk/test numbers) — no environment shares state with another, so testing in staging can never
affect real customer calls.

## 2. Infrastructure Approach

- **Containerized services**: each .NET Core service is packaged as a Docker image, deployed via
  a container orchestrator (Kubernetes, or a managed container service like Azure App Service /
  ECS if a lighter footprint is preferred for the team's size).
- **Infrastructure as Code**: environment infrastructure (networking, databases, container
  clusters) defined in code (e.g., Terraform/Bicep) so staging and production stay consistent and
  environments can be rebuilt reliably.
- **Angular apps** are built into static assets and served via a CDN/static hosting, separate
  from the API deployment pipeline.

## 3. CI/CD Pipeline

1. **Commit / PR** → automated build + unit tests + static analysis run on every PR.
2. **Merge to main** → build produces versioned container images (tagged with commit SHA) and
   pushes them to a container registry.
3. **Auto-deploy to Dev** on every merge, for fast feedback.
4. **Manual promotion to Staging** → run integration tests, load tests (simulating target
   concurrent-agent counts), and manual QA/UAT.
5. **Manual, gated promotion to Production** → requires sign-off; deployed using a
   **blue-green or rolling deployment** strategy so live agents aren't dropped mid-call during a
   release.
6. **Database migrations** run as a separate, explicit pipeline step (not baked silently into app
   startup) so schema changes are reviewable and reversible.

## 4. Release Safety for a Live Call System

Because agents may be on active calls during a deploy, the strategy specifically avoids
"restart everything at once":

- **Rolling/blue-green deploys** for stateless app services — new instances come up and pass
  health checks before old ones are drained and retired.
- **Telephony Orchestration Service deploys are treated with extra care** — in-flight calls should
  not be forcibly dropped; new call setup routes to updated instances while existing calls
  complete on the instance handling them (connection draining).
- **Feature flags** for risky/incomplete features, so a deploy and a feature launch are decoupled.

## 5. Rollback

- Every deployment is a **versioned, immutable container image** — rollback means redeploying the
  last known-good version, not "undoing changes."
- Database migrations are written to be backward-compatible where possible (additive changes first,
  destructive changes in a later, separate release) so a rollback of application code doesn't
  require an emergency schema rollback too.
- A rollback runbook and an on-call rotation are defined before go-live, not improvised during an
  incident.

## 6. Backups & Disaster Recovery

| Data | Backup approach |
|---|---|
| Primary database | Automated daily full backups + continuous transaction log backups (point-in-time restore) |
| Call recordings (object storage) | Versioned/replicated storage; lifecycle policy moves older recordings to cheaper cold storage, not deleted from backup scope |
| Configuration (routing rules, IVR flows) | Stored in the database, covered by the same backup policy; also version-controlled as code where feasible |
| Infrastructure | Reproducible from Infrastructure-as-Code, so a full environment can be rebuilt, not just data restored |

Backups are periodically **test-restored** (not just taken and assumed good) — a backup that's
never been restored isn't a verified backup.

## 7. Monitoring Post-Deploy

- Centralized logging and metrics (e.g., ELK/Prometheus+Grafana or a cloud-native equivalent)
  across all services, with special attention to the telephony path (call setup success rate,
  call setup latency, dropped calls).
- Automated alerting tied to key health indicators (queue depth spiking, call failure rate, CRM
  sync failures) rather than relying on agents/supervisors to notice and report problems.
- A short **post-deploy verification checklist** (can a test call be placed and received, does
  CRM screen-pop work) run after every production deploy, before declaring it complete.
