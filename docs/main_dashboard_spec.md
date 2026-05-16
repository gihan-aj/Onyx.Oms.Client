# Main Dashboard — Design & API Specification
**Onyx.OMS · MVP**

---

## 1. Layout Overview

```
┌──────────────────────────────────────────────────────────────┐
│  HERO BAND  (≈160px, fades to solid at bottom edge)          │
│  Hero image · BlendEffect opacity mask                       │
│  "Good morning, {username}"     [+Order] [Ship] [Tasks] [+Product] │
└──────────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────────┐
│  TODAY AT A GLANCE  — 4 stat tiles (overlap hero bottom)     │
│  [Pending] [Ready to pack] [Tasks ready] [Shipped today]     │
└──────────────────────────────────────────────────────────────┘
┌───────────────────────┬──────────────────────────────────────┐
│  ACTION REQUIRED      │  IN MOTION                           │
│  Pending orders       │  Active production tasks             │
│  Processing (stuck)   │  Active procurement tasks            │
│  Ready to pack (idle) │  Orphaned tasks                      │
│  Delivered, unpaid    │  Shipped / in-transit orders         │
└───────────────────────┴──────────────────────────────────────┘
```

---

## 2. Hero Band

### Dimensions & visual
- Height: **≈160px** (reduced from the current 280px). Enough to show the image and get the fade effect without eating the screen.
- The existing `OpacityMaskView` with the `LinearGradientBrush` fade is kept. The gradient fades the bottom edge of the image to `SolidBackgroundFillColorBase`, so the stat tiles below can visually overlap the edge and float over the faded image — this preserves the look you already have.
- The quick action buttons overlap the bottom of the hero (negative `Margin` on the row below, as you currently do with `-60` margin) so they appear to hover over the faded edge.

### Greeting text (left side of hero)
- Line 1: "Good morning," — `SubtitleTextBlockStyle`, accent color
- Line 2: `{username}` — `TitleTextBlockStyle`, bold, accent tertiary
- Line 3 (new, small): "{date} · {action_count} items need your attention" — `CaptionTextBlockStyle`, secondary color. `action_count` comes from the API.

### Quick actions (right side of hero, same row)
Ordered by frequency of use:

| Order | Label | Icon | Action |
|-------|-------|------|--------|
| 1 | Add order | `&#xE710;` (Add) / fa-cart-plus | Navigate to new order page |
| 2 | Ship orders | `&#xE701;` (Send) / fa-truck | Navigate to orders filtered: ready to ship |
| 3 | Task backlog | `&#xE9D5;` (TaskList) / fa-list-check | Navigate to fulfillment backlog |
| 4 | Add product | `&#xECCD;` (Add to shopping) / fa-box | Navigate to new product page |

Each button: **240×120px** glass acrylic card (your existing `GlassQuickActionButtonStyle`), icon 32px accent color, title `BodyStrongTextBlockStyle`, description `CaptionTextBlockStyle` secondary.

---

## 3. Stat Tiles ("Today at a glance")

Four tiles in a row. Same visual spec as the Catalog Dashboard tiles (see `catalog_dashboard_spec.md` §7). These overlap the hero bottom edge slightly via negative top margin.

| Tile | Metric | Color | Navigation |
|------|--------|-------|------------|
| Pending confirmation | Count of orders in `Pending` status | Amber | Orders filtered: `status=Pending` |
| Ready to pack | Count of orders in `Ready to Pack` status | Teal | Orders filtered: `status=ReadyToPack` |
| Tasks completed | Count of fulfillment tasks in `Ready` status not yet allocated | Blue | Task backlog filtered: `status=Ready` |
| Shipped today | Count of orders moved to `Shipped` today | Blue | Orders filtered: `status=Shipped` |

---

## 4. Action Required Panel

This panel surfaces orders and situations that are **stuck** and need the user to do something. Shown as a vertical list of cards, max ~5 items, with a "View all" link.

### What qualifies as "action required"

| Condition | Why it matters | Badge label |
|-----------|---------------|-------------|
| Order status = `Pending` | Saved but not confirmed yet — needs payment decision | Pending |
| Order status = `Confirmed` or `Processing`, no task activity in N hours | Might be stalled — tasks not started | Processing |
| Order status = `Ready to Pack`, created more than X hours ago | Been sitting idle, should have been packed | Ready to pack |
| Order status = `Delivered`, payment status = `Partially Paid` | Customer hasn't settled balance | Unpaid balance |
| Order status = `Returned to Sender` | Physical goods returned, needs re-decision | RTO |

> **Thresholds for MVP**: "stalled" and "idle" thresholds can be hardcoded server-side for now (e.g. 24h for stalled processing, 12h for idle ready-to-pack). These can be made configurable later.

### Card layout (each item)
- Left: icon in a colored rounded square (color matches badge semantic)
- Middle: order number + customer name (bold), status context (caption, muted)
- Right: order total amount, status badge pill

### Sort order
1. `Returned to Sender` (most urgent — physical goods at risk)
2. `Delivered + Unpaid` (money outstanding)
3. `Pending` (unconfirmed)
4. `Ready to Pack` idle (oldest first)
5. `Processing` stalled (oldest first)

---

## 5. In Motion Panel

Shows things that are **actively progressing** — no action needed, just visibility.

### What shows here

| Item type | Condition | Display |
|-----------|-----------|---------|
| Production task | Status = `In Production` | Variant name, unit count, linked order if any |
| Procurement task | Status = `Ordered` | Variant name, unit count, supplier note |
| Orphaned task | Any active task with no linked order | Variant name, "completes to stock" note |
| Shipped order | Status = `Shipped` | Customer name, tracking reference |

### Card layout (each item)
- Left: colored dot (teal = production, blue = procurement, amber = orphaned, teal = shipped)
- Middle: item/order name, context line (muted caption)
- Right: unit count (for tasks) or badge (for orders)

### Sort order
- Tasks in `In Production` or `Ordered` first, then shipped orders, then orphaned tasks last.

---

## 6. Required APIs

### 6.1 `GET /api/dashboard/summary`

Returns all data for the hero greeting line, the 4 stat tiles, and the action-required count badge.

**Query parameters:** none

**Response body:**

```json
{
  "user": {
    "display_name": "Ashan"
  },
  "action_required_count": 3,
  "stats": {
    "pending_orders": 5,
    "ready_to_pack": 8,
    "tasks_completed_unallocated": 3,
    "shipped_today": 12
  }
}
```

**Notes:**
- `action_required_count` is the total count of items that would appear in the Action Required panel. Computed server-side using the same conditions defined in §4. This drives the "N items need your attention" line in the hero greeting.
- `shipped_today` counts orders where the status transitioned to `Shipped` on the current calendar day (server timezone).
- `tasks_completed_unallocated` counts fulfillment tasks in `Ready` status that have not yet caused their linked order item to be marked `Ready` — i.e., the task finished but the user hasn't progressed the order yet. For orphaned tasks, all `Ready` ones count.

---

### 6.2 `GET /api/dashboard/action-required`

Returns the list of items for the Action Required panel.

**Query parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `limit` | int | No | Max items to return. Default: `5` |

**Response body:**

```json
{
  "total": 3,
  "items": [
    {
      "type": "order",
      "order_id": "ord_038",
      "order_number": 1038,
      "customer_name": "Nimal Silva",
      "total_amount": 2200.00,
      "currency": "LKR",
      "status": "Delivered",
      "reason": "unpaid_balance",
      "reason_label": "Delivered · Balance unpaid",
      "created_at": "2026-05-10T09:14:00Z"
    },
    {
      "type": "order",
      "order_id": "ord_042",
      "order_number": 1042,
      "customer_name": "Kamal Perera",
      "total_amount": 4800.00,
      "currency": "LKR",
      "status": "Pending",
      "reason": "pending_confirmation",
      "reason_label": "Saved · Not yet confirmed",
      "created_at": "2026-05-15T07:30:00Z"
    },
    {
      "type": "order",
      "order_id": "ord_035",
      "order_number": 1035,
      "customer_name": "Sanduni Dias",
      "total_amount": 6100.00,
      "currency": "LKR",
      "status": "ReadyToPack",
      "reason": "idle_ready_to_pack",
      "reason_label": "Ready to pack · Since yesterday",
      "created_at": "2026-05-14T11:00:00Z"
    }
  ]
}
```

**`reason` enum values:**

| Value | Meaning |
|-------|---------|
| `returned_to_sender` | Order bounced back from courier |
| `unpaid_balance` | Delivered but not fully paid |
| `pending_confirmation` | Order in `Pending` status |
| `stalled_processing` | In `Processing` with no task progress |
| `idle_ready_to_pack` | `Ready to Pack` sitting idle past threshold |

**Notes:**
- `reason_label` is a pre-composed human-readable string for display. Compose server-side.
- Items are returned pre-sorted by urgency (see §4 sort order).
- Navigation on click: go to the order detail page for `order_id`.

---

### 6.3 `GET /api/dashboard/in-motion`

Returns the list of items for the In Motion panel.

**Query parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `limit` | int | No | Max items to return. Default: `5` |

**Response body:**

```json
{
  "total": 4,
  "items": [
    {
      "type": "task",
      "task_id": "task_081",
      "variant_label": "Linen Shirt · XL · Navy",
      "task_type": "Production",
      "task_status": "InProduction",
      "quantity": 12,
      "linked_order_number": 1042,
      "linked_order_id": "ord_042",
      "is_orphaned": false,
      "context_label": "Started 2 days ago · linked to #1042"
    },
    {
      "type": "task",
      "task_id": "task_077",
      "variant_label": "Polo Tee · M · White",
      "task_type": "Production",
      "task_status": "InProduction",
      "quantity": 8,
      "linked_order_number": null,
      "linked_order_id": null,
      "is_orphaned": true,
      "context_label": "No linked order · completes to stock"
    },
    {
      "type": "order",
      "order_id": "ord_044",
      "order_number": 1044,
      "customer_name": "Fathima Nawaz",
      "status": "Shipped",
      "tracking_number": "YD-00192837",
      "context_label": "With courier · tracking assigned"
    }
  ]
}
```

**`dot_color` hint** — the client should map these to the dot color:

| `task_type` + `is_orphaned` | Dot color |
|---------------------------|-----------|
| `Production`, not orphaned | Teal |
| `Procurement`, not orphaned | Blue |
| Any, `is_orphaned = true` | Amber |
| `type = order`, status `Shipped` | Teal |

**Notes:**
- `context_label` is pre-composed server-side for display.
- For tasks, `linked_order_number` is `null` if orphaned.
- Navigation on click: task → fulfillment task detail; order → order detail.

---

## 7. Hero Image & Blend Effect (preserving existing behavior)

Keep the current `OpacityMaskView` + `LinearGradientBrush` approach. Suggested adjustments:

```xml
<!-- Reduce hero height from 280 to 160 -->
<Grid Grid.Row="0" Height="160">
  <controls:OpacityMaskView Height="160" VerticalAlignment="Stretch">
    ...
  </controls:OpacityMaskView>

  <!-- Greeting: move to bottom-left, reduce bottom margin since hero is shorter -->
  <StackPanel VerticalAlignment="Bottom" Margin="40,0,0,56">
    <TextBlock Text="Good morning," ... />
    <TextBlock Text="{x:Bind ViewModel.UserName}" ... />
    <TextBlock Text="{x:Bind ViewModel.HeroSubLabel}" ... /> <!-- "15 May · 3 items need attention" -->
  </StackPanel>
</Grid>

<!-- Quick actions: keep negative margin overlap, adjust to -48 or -52 -->
<Grid Grid.Row="1" Margin="40,-52,40,24">
  ...
</Grid>
```

The overlap of the quick action cards over the faded bottom edge is what creates the layered depth effect — keep this. With the hero at 160px, the bottom 40–50px fades to solid, and the acrylic quick-action cards hover over exactly that zone.

---

## 8. Data Loading Strategy

1. On page navigate: fire `GET /api/dashboard/summary` immediately — renders the hero sub-label (`action_required_count`) and all 4 stat tiles.
2. Fire `GET /api/dashboard/action-required?limit=5` and `GET /api/dashboard/in-motion?limit=5` in parallel.
3. Render both panels when their responses arrive (show shimmer skeletons in the meantime).
4. No auto-polling for MVP. Add a manual refresh button in the header band if needed.

---

## 9. ViewModel Properties (suggested additions)

```csharp
// Hero
public string UserName { get; }
public string HeroSubLabel { get; }       // e.g. "Friday, 15 May · 3 items need your attention"

// Stat tiles
public int PendingOrderCount { get; }
public int ReadyToPackCount { get; }
public int CompletedTaskCount { get; }
public int ShippedTodayCount { get; }

// Panels
public ObservableCollection<ActionRequiredItem> ActionRequiredItems { get; }
public ObservableCollection<InMotionItem> InMotionItems { get; }
public bool IsLoadingActionRequired { get; }
public bool IsLoadingInMotion { get; }
```

---

*Document version: MVP · Onyx.OMS Main Dashboard*