# Scalability Plan — 50 to 500+ Agents

## 1. What Actually Changes Between 50 and 500 Agents

Going 10x isn't just "add more servers" — a few things behave non-linearly:

- **Concurrent call volume** scales with agent count, so telephony capacity (SIP trunk channels)
  and media handling need to scale with it, not just the app tier.
- **Real-time state (Redis, SignalR)** sees far more churn — status changes, queue updates.
- **Routing decisions** need to stay fast (sub-second) even with many more agents and queues to
  evaluate against.
- **Reporting queries** get heavier as call history grows into the millions of rows.
- **Recording storage** grows linearly and becomes a real cost line, not a rounding error.

## 2. How the Architecture Already Supports This

Because the core services (Routing, CRM Integration, Recording, Reporting, Identity) are
**stateless** and sit behind the API Gateway, they scale horizontally by adding instances — no
redesign needed. This is the main reason the MVP architecture was built this way from day one
rather than optimizing only for 50 agents.

## 3. Scaling Each Layer

| Layer | 50 agents | Path to 500+ agents |
|---|---|---|
| Angular clients | N/A (client-side) | No change — served via CDN |
| API Gateway | Single instance/cluster | Horizontally scaled, load-balanced |
| App services (.NET Core) | Small container cluster | Auto-scaling container group (e.g., Kubernetes/App Service scale rules) per service, scaled independently by load |
| Call Session state (Redis) | Single Redis instance | Redis cluster/managed Redis with replication |
| SignalR | In-process | Backplane (Redis or Azure SignalR Service) so notifications work across multiple gateway instances |
| Message broker | Single broker instance | Clustered broker, partitioned topics by call/queue if needed |
| Primary DB (Postgres) | Single instance | Read replicas for reporting queries; partition/archive old call data; connection pooling |
| Telephony (SIP trunk) | Enough channels for 50 concurrent calls | Scale trunk capacity/channels with the provider; this is a commercial/capacity conversation, not just code |
| Recording storage | Standard object storage | Lifecycle policies — hot storage for recent recordings, cold/archive tier for older ones |
| Reporting | Direct queries against primary DB | Move to a read replica or a separate reporting/warehouse store (e.g., nightly ETL) once volume grows |

## 4. Specific Bottlenecks to Watch

1. **Routing Engine latency under load** — mitigate by keeping routing rules in memory/cache
   (Redis) rather than hitting the database per call.
2. **CRM API rate limits** — the CRM Integration Service should queue and retry writes rather than
   assume the CRM can absorb 500 agents' worth of real-time traffic; batch where the CRM allows it.
3. **SignalR fan-out** — with 500 agents and supervisors, broadcasting queue updates to everyone is
   wasteful; scope notifications to relevant teams/queues instead of global broadcast.
4. **Database growth from call history** — plan for partitioning/archiving call records early
   (e.g., by month) so query performance doesn't degrade as data grows.

## 5. Non-Architectural Scaling Considerations

- **Telephony capacity is a procurement/cost conversation**, not just an engineering one — going
  from 50 to 500 concurrent agents means renegotiating trunk capacity with the provider well ahead
  of the growth, not reactively.
- **Operational scaling**: more agents means more supervisors, more IVR complexity, more routing
  rules — the admin tooling needs to stay usable at that scale (e.g., searchable agent/queue lists,
  not just a flat page that worked fine at 50).
- **Monitoring/alerting thresholds** need to be revisited — what counted as "high queue depth" at
  50 agents isn't the same number at 500.

## 6. What We're NOT Doing at MVP Stage

We are not pre-building multi-region failover or auto-scaling infrastructure for 500 agents on
day one — that would slow down the MVP for capacity we don't need yet. Instead, the MVP is built
so that the *scaling path* is additive (more instances, bigger Redis, trunk capacity) rather than
requiring an architectural rewrite. That distinction — "ready to scale" vs. "pre-scaled" — is the
core of this plan.
