\# SaaS Platform Project



Production-ready SaaS platform architecture built with \*\*ASP.NET Core\*\*, featuring an \*\*API Gateway\*\*, \*\*Canary Routing\*\*, \*\*Rate Limiting\*\*, \*\*Prometheus monitoring\*\*, and \*\*automatic rollback\*\*.



\## 🚀 Architecture Overview



\- API Gateway (Ocelot)

\- Multi-tenant support

\- Redis-based rate limiting \& quota tracking

\- Canary routing with weighted traffic split

\- Prometheus metrics \& alerting

\- Alertmanager-triggered automatic rollback

\- Clean Architecture (Domain / Application / Infrastructure)



\## 🧩 Key Features



\- \*\*API Gateway\*\*

&nbsp; - JWT authentication

&nbsp; - Rate limiting

&nbsp; - Load balancing

&nbsp; - Canary traffic routing



\- \*\*Canary Deployment\*\*

&nbsp; - % traffic split (e.g. 95% stable / 5% canary)

&nbsp; - Canary error \& latency metrics

&nbsp; - Automatic rollback via Alertmanager webhook



\- \*\*Observability\*\*

&nbsp; - Prometheus metrics

&nbsp; - Custom canary metrics

&nbsp; - Alert rules for errors, latency, quota, rate limits



\- \*\*Multi-Tenancy\*\*

&nbsp; - Tenant-aware request handling

&nbsp; - Per-tenant quota \& rate limit enforcement



\## 🛠️ Tech Stack



\- ASP.NET Core

\- Ocelot API Gateway

\- Redis

\- Prometheus \& Alertmanager

\- Entity Framework Core

\- Clean Architecture

\- Docker-ready design



\## 📈 Why This Project?



This project demonstrates \*\*real-world backend engineering practices\*\*:

\- Production-grade API Gateway

\- Traffic-safe deployments (canary)

\- Observability-first mindset

\- Automated rollback strategies



Perfect for showcasing \*\*senior-level system design skills\*\*.



\## 📌 Status



🚧 Actively developed – new improvements planned:

\- Ocelot hot config reload

\- Tenant-based canary routing

\- Slack / Teams notifications



---



👤 \*\*Author:\*\* Berk Turan

