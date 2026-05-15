# Catalog Dashboard — Design & API Specification
**Onyx.OMS · MVP**

---

## 1. Overview

The Catalog Dashboard is a read-only inventory snapshot page. It gives the user a fast, at-a-glance view of the health of their stock without navigating into individual products. It follows a **Windows lock-screen widget** visual pattern: compact tiles with a large hero metric, a muted label, and a small colored accent chip at the bottom that navigates to a relevant filtered page.

---

## 2. Page Layout

```
┌─────────────────────────────────────────────────────────┐
│  Catalog & Inventory                                    │
│  Live snapshot · Last synced just now                   │
├──────────┬──────────┬──────────┬──────────┐             │
│ Variants │ Out of   │ Low      │ Inbound  │  ← Row 1   │
│ (total)  │ Stock    │ Stock    │ Stock    │  Small tiles│
├──────────┴──┬────────────────┴──┐         │             │
│  Stock on   │  Active           │          │  ← Row 2   │
│  Hand       │  Fulfillment      │          │  Wide tiles │
│  breakdown  │  Tasks            │          │             │
├─────────────┼───────────────────┤          │             │
│  Out of     │  Low Stock        │          │  ← Row 3   │
│  Stock list │  list             │          │  Alert panels│
└─────────────┴───────────────────┘          │             │
```

### 2.1 Tile anatomy

Each small tile (approx. **160 × 110px**):
- **Top label**: 11px, muted, uppercase — describes the metric
- **Hero number**: 34px, weight 500 — the primary value
- **Sub-label**: 12px, secondary color — provides context
- **Accent chip**: 10px pill at the bottom, colored by semantic meaning, shows destination on click

Wide tiles (approx. **330 × 110px**):
- Same anatomy but contain **3 sub-stats** side by side with dividers
- Useful for related but distinct values (e.g. total / reserved / available)

Alert panels:
- A list of up to 3 worst-offending variants (zero stock or lowest count)
- Each row: variant name + a colored badge showing quantity
- Clicking the panel header navigates to the filtered product list

---

## 3. Tile Definitions

### Row 1 — Overview Tiles

| Tile | Metric | Color accent | Navigation destination |
|------|--------|-------------|------------------------|
| Total variants | Count of all `ProductVariant` records | Blue | Products list (no filter) |
| Out of stock | Count of variants where `AvailableStock = 0` | Red | Products list filtered: `available=0` |
| Low stock | Count of variants where `0 < AvailableStock ≤ threshold` | Amber | Products list filtered: `low_stock=true` |
| Inbound stock | Two sub-values: variant count + total units where `InboundStock > 0` | Teal | Fulfillment task backlog |

### Row 2 — Wide Tiles

| Tile | Sub-stats | Color accent | Navigation destination |
|------|-----------|-------------|------------------------|
| Stock on hand | `StockOnHand` total / `ReservedStock` / `AvailableStock` | Blue | Full stock report / products list |
| Active fulfillment tasks | In production count / Procurement count | Purple | Task backlog |

> **Note:** Orphaned task count removed from MVP per product decision.

### Row 3 — Alert Panels

| Panel | Content | Navigation destination |
|-------|---------|------------------------|
| Out of stock | Top 3 variants with `AvailableStock = 0`, sorted by product name | Products page filtered: `available=0` |
| Low stock | Top 3 variants with lowest `AvailableStock` above 0, up to threshold | Products page filtered: `low_stock=true` |

---

## 4. Required APIs

### 4.1 `GET /api/catalog/dashboard/summary`

Returns all data needed to populate Rows 1 and 2. Single request on page load.

**Query parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `low_stock_threshold` | `int` | Yes | Global threshold for "low stock" classification. Sent by client from app settings. |

**Response body:**

```json
{
  "total_variant_count": 248,
  "out_of_stock_count": 14,
  "low_stock_count": 31,
  "inbound": {
    "variant_count": 19,
    "total_units": 89
  },
  "stock_totals": {
    "stock_on_hand": 1842,
    "reserved_stock": 307,
    "available_stock": 1535
  },
  "fulfillment_tasks": {
    "in_production": 12,
    "procurement": 7
  }
}
```

**Notes:**
- `available_stock` in the response is a pre-computed sum of all `AvailableStock` = (`StockOnHand` - `ReservedStock`) across all variants. This is read-only and never stored.
- `inbound.variant_count` = count of distinct variants with at least one active fulfillment task currently incrementing `InboundStock`.
- `inbound.total_units` = sum of `InboundStock` across all such variants.
- This is a lightweight aggregation query. Index on `AvailableStock` (computed or stored) and `InboundStock` for performance.

---

### 4.2 `GET /api/catalog/dashboard/alerts`

Returns the alert panel data for Row 3. Can be loaded after the summary (lower priority, non-blocking).

**Query parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `low_stock_threshold` | `int` | Yes | Same global threshold |
| `limit` | `int` | No | Max variants per alert group. Default: `3` |

**Response body:**

```json
{
  "out_of_stock": [
    {
      "product_id": "prod_001",
      "product_name": "Linen Shirt",
      "variant_id": "var_014",
      "variant_label": "XL · Navy",
      "available_stock": 0
    }
  ],
  "low_stock": [
    {
      "product_id": "prod_007",
      "product_name": "Cargo Shorts",
      "variant_id": "var_031",
      "variant_label": "L · Olive",
      "available_stock": 2
    }
  ]
}
```

**Notes:**
- `out_of_stock` sorted by `product_name` ASC (alphabetical, consistent).
- `low_stock` sorted by `available_stock` ASC (worst offenders first).
- `variant_label` is a display string combining the variant's attribute values (e.g. size + color). Compose this server-side so the UI doesn't need to reconstruct it.
- Navigation from these rows goes to the **product detail page** (since variants aren't shown as a standalone list in the current UI). Pass `product_id` as the navigation target, and optionally `variant_id` to scroll/highlight that variant on the product page.

---

## 5. Navigation & Click Destinations

Since variants are viewed inside the product detail page (not a standalone list), all navigation from the dashboard goes to one of two places:

| Chip label | Destination | Filter/param passed |
|------------|-------------|---------------------|
| View catalog | `/products` | none |
| View stockouts | `/products` | `?filter=out_of_stock` |
| View alerts | `/products` | `?filter=low_stock&threshold={n}` |
| View tasks | `/fulfillment/tasks` | none |
| Full stock report | `/products` | none (or future stock report page) |
| Task backlog | `/fulfillment/tasks` | none |
| Alert panel (out of stock) | `/products/{product_id}` | optional `#variant_{variant_id}` anchor |
| Alert panel (low stock) | `/products/{product_id}` | optional `#variant_{variant_id}` anchor |

The products list page should be able to accept and apply the `filter` query param on load. This requires a small addition to the product list page if not already implemented.

---

## 6. Stockout Timestamp Tracking (Post-MVP)

> You noted the system currently cannot track *when* a stockout happened. Here's a practical way to implement it.

### The problem

`AvailableStock` is a calculated value (`StockOnHand - ReservedStock`). It has no history. You only know the current value, not when it crossed zero.

### Recommended approach: event-stamping on the variant

Add two nullable fields to the `ProductVariant` entity:

```
StockedOutAt   : DateTime?   // set when AvailableStock first hits 0
RestockedAt    : DateTime?   // set when AvailableStock returns above 0
```

Then, in any service method that mutates `StockOnHand` or `ReservedStock` on a variant, add a check **after** the mutation:

```
previous_available = StockOnHand_before - ReservedStock_before
new_available      = StockOnHand_after  - ReservedStock_after

if previous_available > 0 AND new_available <= 0:
    variant.StockedOutAt = DateTime.UtcNow
    variant.RestockedAt  = null

if previous_available <= 0 AND new_available > 0:
    variant.RestockedAt  = DateTime.UtcNow
    variant.StockedOutAt = null   // optional: clear it on restock
```

This logic belongs in whatever layer manages stock mutations — likely a `InventoryService` or within your fulfillment completion and order confirmation handlers.

### What this unlocks

- Dashboard can show **"Out of stock for N days"** under the hero number in the tile
- You can sort the alert panel by `StockedOutAt` (longest-suffering variants first)
- Future: a report of stockout frequency per variant (how often does a SKU go to zero?)
- Future: alert rules like "notify if a variant has been out of stock for > 3 days"

### Migration note

On first deploy, set `StockedOutAt = migration_date` for any variant already at `AvailableStock = 0`, so the data is not misleadingly null.

---

## 7. Design Tokens (WinUI3 alignment)

These values match the visual spec of the mockup and should translate naturally into WinUI3 styles.

| Property | Value |
|----------|-------|
| Tile corner radius | `12px` / `CornerRadius="12"` |
| Tile border | `0.5px` / thin `BorderThickness` with muted color |
| Hero number font size | `34–36px` |
| Hero number font weight | `500` (Medium) |
| Label font size | `11px` |
| Sub-label font size | `12px` |
| Accent chip font size | `10px` |
| Accent chip corner radius | `20px` (pill) |
| Tile min-height | `110px` |
| Wide tile min-height | `110px` |
| Grid gap | `10px` |
| Tile padding | `14px 16px 10px` |
| Surface color | Acrylic / `--color-background-primary` |

### Chip colors by semantic meaning

| State | Chip background | Chip text |
|-------|----------------|-----------|
| Neutral / info | Blue-50 `#E6F1FB` | Blue-600 `#185FA5` |
| Warning / low | Amber-50 `#FAEEDA` | Amber-600 `#854F0B` |
| Danger / zero | Red-50 `#FCEBEB` | Red-600 `#A32D2D` |
| Positive / inbound | Teal-50 `#E1F5EE` | Teal-600 `#0F6E56` |
| Action / tasks | Purple-50 `#EEEDFE` | Purple-600 `#3C3489` |

---

## 8. Data Loading Strategy

1. On page navigate: fire `GET /api/catalog/dashboard/summary?low_stock_threshold={n}` immediately.
2. Render Row 1 and Row 2 tiles as soon as the response arrives.
3. Fire `GET /api/catalog/dashboard/alerts?low_stock_threshold={n}&limit=3` in parallel (or after summary).
4. Render Row 3 alert panels when alerts response arrives (show a skeleton/shimmer in the meantime).
5. Optionally: add a manual refresh button. No auto-polling needed for MVP.

---

*Document version: MVP · Onyx.OMS Catalog Dashboard*