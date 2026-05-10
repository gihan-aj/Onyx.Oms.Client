# Orders Page Design & Filtering Discussion

This document captures the design decisions, filtering logic, and UI suggestions for the Orders Page in the WinUI client.

## 1. WinUI Design Suggestions (Filtering & Tabs)

### Tabbed View for High-Level Grouping
To avoid overwhelming the user, the UI should use a TabView at the top of the page to group orders by their lifecycle stage.
- **Tab 1: All Active** (Default) - Shows all actionable orders.
- **Tab 2: Pending** - Orders waiting for confirmation or payment.
- **Tab 3: Processing** - Orders currently in production or procurement.
- **Tab 4: Ready to Pack** - Orders waiting to be packed.
- **Tab 5: Packed** - Orders waiting for courier pickup.
- **Tab 6: Historical** - Shipped, Delivered, Completed, or Cancelled orders.

*UI Tip:* Fetch the counts for each status from the API and display them in the Tab headers (e.g., `Processing (5)`).

### Status Toggle Chips
Within the "All Active" tab, we can use WinUI `ToggleButton` controls styled as rounded "Chips".
- These chips correspond to individual statuses (Pending, Confirmed, Processing, etc.).
- **Default State:** The chips for actionable statuses (`Pending`, `Confirmed`, `Processing`, `ReadyToPack`, `Packed`, `PaymentFailed`) are pre-selected (toggled ON).
- The user can click a chip to toggle that specific status off or on, instantly refining the data grid below.
- *API Requirement:* This requires updating the `GetOrdersPagedQuery` to accept a `List<OrderStatus>? Statuses` instead of a single `Status`.

### Date Range Filtering
- Add `FromDate` and `ToDate` DatePickers.
- **Default:** Default the date range to the **past 30 days** (or the start of the current month to today). 
- When the date range changes, the API should re-fetch both the paged order data *and* the counts for the tabs.

## 2. API & Query Refinements

### Useful Filters to Add to the API
- `List<OrderStatus>? Statuses` (Replacing the single `Status` enum to support the chips UI).
- `bool? IsCashOnDelivery` - Useful for logistics teams printing waybills or separating payment workflows.
- `Guid? CourierId` - Useful for batch handovers to specific couriers once orders are packed.
- `string? PrimaryPhone` - Should be added to the `SearchTerm` logic, as customers often reference their phone numbers via WhatsApp/Social Media inquiries.

### Default Sorting
- **Actionable View (Active Orders):** Consider sorting by `OrderDate ASC` (Oldest first). This pushes unresolved, stale orders to the top of the queue so they aren't ignored by staff.
- **Historical View:** Sort by `OrderDate DESC` (Newest first) for general browsing.

## 3. Clarification: `OrderDate` vs `CreatedOnUtc`

In the domain model, there is both an `OrderDate` and a `CreatedOnUtc`. 

- **`CreatedOnUtc`**: This is a strict system-level audit field. It records the exact millisecond the record was inserted into the database. It is immutable and strictly tracks system activity.
- **`OrderDate`**: This is a **business-level field**. In an OMS dealing with social media orders, a customer might place an order on WhatsApp on a Friday night, but the staff might not type it into the OMS until Monday morning. 
  - `OrderDate` would be set to *Friday* (when the sale actually happened).
  - `CreatedOnUtc` would be *Monday* (when the data entry happened).
  - `OrderDate` can be editable by the user during creation, while `CreatedOnUtc` is handled automatically by the system.

When filtering by "From" and "To" dates in the UI, you should filter against **`OrderDate`**, as that reflects the business reality of when sales occurred.
