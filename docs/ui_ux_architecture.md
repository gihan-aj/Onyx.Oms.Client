# UI/UX Architecture & Design Patterns - Onyx.Oms Client

## 1. Core Design Philosophy: Task-Based UI
The Onyx.Oms WinUI 3 client strictly follows a **Task-Based User Interface** paradigm. We avoid monolithic "Edit" pages with a single global "Save" button. 



Instead, the UI is designed around user intent and atomic actions. 
* Data is primarily presented in a read-only format.
* Modifications are made via explicit command buttons (e.g., "Confirm Order", "Log Payment", "Mark as Packed").
* Each UI action maps directly to a specific backend Use Case (Vertical Slice command).
* This ensures data integrity and prevents concurrent modification conflicts.

## 2. Information Architecture (Navigation)
The application uses a standard WinUI `NavigationView` (left sidebar) to separate major business functions into logical sections.

### Overview
* **Dashboard:** High-level metrics and alerts.

### Sales
* **Orders:** The primary entry point for the sales flow.
* **Customers:** Directory and order history per customer.

### Operations
* **Fulfillment:** The execution area for production and procurement tasks.
* **Catalog:** Product, Category, and Variant management overview.
    * **Categories:** Manage product classifications.
    * **Products:** Manage master product records.
    * **Variants:** Manage individual SKUs, pricing, and specific options.
* **Couriers:** Management of shipping providers and logistics partners.

### Administration
* **Users:** Management of system users and invitations.
* **Roles:** Security, access control, and user role management.

## 3. The Order Flow: Creation vs. Management
To reduce complexity, creating an order and managing an existing order are handled by entirely separate pages and view models.

### 3.1 Order Creation Page (`CreateOrderPage.xaml`)
* **Purpose:** High-speed data entry for new intake (e.g., transcribing a WhatsApp order).
* **Behavior:** A streamlined form. 
* **Key Interactions:**
    * **Customer Selection:** User searches for an existing customer or opens a flyout/dialog to create a new one on the fly. *Note: The customer cannot be changed once the order is saved.*
    * **Item Entry:** A searchable combo box for Product Variants, a quantity input, and an "Add to List" button. The system permits adding items regardless of stock levels.
    * **Completion:** A single "Save as Pending" button commits the draft order to the database and immediately navigates the user to the Order Details page.

### 3.2 Order Details Page (`OrderDetailPage.xaml`)
* **Purpose:** The "Command Center" for a specific order. 
* **Behavior:** Primarily a read-only layout broken into distinct logical cards or sections, decorated with context-specific action buttons.



#### UI Sections & Atomic Actions
* **Header:** Displays Order ID, Date, and prominent Status Badges (Order Status, Payment Status).
    * *Actions:* `Confirm Order` (validates COD or payment presence), `Cancel Order`.
* **Customer Info Card:** Shows shipping details.
    * *Actions:* `Edit Shipping Address` (opens a `ContentDialog`).
* **Financials & Payments Card:** Shows total, advance paid, and balance.
    * *Actions:* `Log Payment` (opens a `ContentDialog` to enter amount, date, and reference).
* **Order Items Grid:** * Visually grouped by status: **Ready/Allocated** vs. **Action Required** (To Be Produced / To Be Procured).
    * Uses visual cues (icons/colors) to indicate stock shortages.
    * *Actions:* Contextual buttons on specific rows (e.g., a "Mark Ready" button for an item that was in production).

## 4. Fulfillment & Production Dashboard
While the Order Details page shows the *status* of items, the actual *work* of making or acquiring items is managed in a separate global view.

### 4.1 Global Task View (`FulfillmentPage.xaml`)
* **Purpose:** Allows production/warehouse staff to see aggregated demand without clicking into individual orders.
* **Layout:** A grouped `ItemsRepeater` or multi-column layout (similar to a Kanban board).
* **Work Streams:**
    * **Production Backlog:** Lists all items across all orders with status "To Be Produced", aggregated by Product Variant (e.g., "Need 5x Red Medium T-Shirts total").
    * **Procurement Backlog:** Lists all items across all orders with status "To Be Procured".
* **Interactions:**
    * Users select a variant and click "Start Production" or "Mark as Ordered". This fires a backend command that updates the state of all associated `OrderItem` records across their respective parent orders.

## 5. Client State Management
To handle complex data flows, cross-page communication, and draft states, the application utilizes a centralized **State Management Service** (`IOrderStateService`), injected via Dependency Injection.

### 5.1 Responsibilities of the State Service
* **Cross-Page Navigation:** Acts as a secure conduit for passing context (e.g., `CurrentOrderId`) between the Order List page and the Order Details page, avoiding fragile UI frame parameters.
* **Draft Order State ("The Cart"):** During Order Creation, the service holds an `ObservableCollection` of selected product variants, quantities, and the selected `Customer` before they are committed to the database.
* **Dialog Synchronization:** UI components like the "Add Product" dialog bind to the State Service. This allows the dialog to accurately reflect real-time status (e.g., visually indicating which variants are already in the current order draft) and seamlessly push new selections to the underlying page view.

### 5.2 State Lifecycle & Isolation
* To prevent data leakage between sessions, the State Service enforces a strict `Reset()` protocol. 
* When navigating away from an order or successfully saving a draft, the service's internal state is completely cleared, ensuring the next operation starts with a clean slate.
* **Context Awareness:** The service explicitly differentiates between a "Draft Context" (creating a new order in memory) and an "Execution Context" (viewing an existing order where additions trigger immediate backend commands).