# Business Logic & System Documentation - Onyx.Oms

## 1. Introduction
This document outlines the business logic, domain entities, workflows, and rules governing the Onyx Order Management System (OMS). It serves as a reference for the development team to ensure a shared understanding of the system's behavior and intent.

**Target Audience:** Developers, Product Owners, QA, Stakeholders.

## 2. System Overview
Onyx.Oms is an Order Management System tailored for a clothing business that receives orders primarily through social media channels (Facebook, WhatsApp). The system handles product catalog management, order processing, inventory tracking, basic production/procurement task management for out-of-stock items, manual payment tracking, shipping fee calculation with zone rates, and integration with local couriers for shipping.

## 3. Domain Entities

### 3.1 Catalog
- **Category**: Hierarchical product categories (e.g., Category -> Subcategory -> Sub-subcategory).
- **Product**: The base product definition.
- **Product Variant**: A specific variation of a product (e.g., Size, Color) which tracks its own stock quantity.

### 3.2 Sales
- **Customer**: The buyer. Can be selected or created on the fly during order placement.
- **Order**: Represents a customer purchase. It associates a customer with multiple order items.
- **Order Item**: A specific product variant within an order, along with its requested quantity.
- **Payment**: A manual record of money received for a specific order.

### 3.3 Fulfillment
- **Fulfillment Task (or Production Task)**: Represents the work required to produce or acquire items that were out of stock when an order was confirmed.

## 4. Status Definitions

### 4.1 Order Statuses
- **Pending**: Initial state when the order is saved but not yet confirmed with the customer.
- **Confirmed**: Order is confirmed. Payment criteria are met (COD or advance paid). Stock is allocated or tasks are created for missing items.
- **Processing**: Work has started on fulfillment tasks for missing items.
- **Ready to Pack**: All order items are ready for fulfillment.
- **Packed**: Items are packed, and a courier has been assigned.
- **Shipped**: Handed over to the courier; a tracking number is assigned.
- **Delivered**: Courier has successfully delivered the package.
- **Completed**: Order is delivered AND fully paid.
- **Cancelled** = Order is cancelled.
- **Return In Transit** = Courier is returning,
- **Returned To Sender** = Package is returned,
- **Return Processed**: Package is processed and added good items back to inventory,
- **Lost In Transit**: Package is lost while returning,
- **DeliveryFailed**: Could not deliver and items are lost,

### 4.2 Order Item Statuses
*These statuses help handle the complexity of out-of-stock items by distinguishing how they will be fulfilled.*
- **Allocated**: Item is currently in stock and has been reserved for this order.
- **To Be Produced**: Item is out of stock but can be made internally. A production task is pending.
- **To Be Procured**: Item is out of stock and needs to be acquired from a supplier. A procurement task is pending.
- **In Production**: User has started the internal task to make this item.
- **Ordered (Procurement)**: User has placed an order with a supplier to acquire this item.
- **Ready**: The item has been made/acquired and is ready to be packed.

### 4.3 Payment Statuses
- **Unpaid**: No payments have been recorded.
- **Partially Paid**: An advance payment has been recorded, but a balance remains.
- **Fully Paid**: The total order amount has been received.

## 5. Key Workflows

### 5.1 Order Placement & Confirmation
1. **Intake**: A customer messages via FB/WhatsApp requesting items.
2. **Creation**: The user opens the Orders page, selects or creates the Customer, and adds Product Variants to the order. *System allows any quantity to be added, regardless of current stock.*
3. **Save**: The order is saved as **Pending**.
4. **Confirmation Check**: The user confirms the payment method (Cash on Delivery or Advance Payment).
5. **Confirmation**: The user clicks 'Confirm Order'. The system validates business rules (see Section 6).
6. **Stock Allocation**: 
   - Available items are reserved.
   - For missing items, a clear notification is shown, and the system flags them as requiring **Fulfillment Tasks**.
   - An invoice can be optionally sent to the customer's email.

### 5.2 Fulfillment (Production/Procurement)
1. **Task Visibility**: The user sees products that need to be fulfilled on a Dashboard, separated into two key areas:
   - **Production Backlog**: Items with status **To Be Produced**.
   - **Procurement Backlog / Reorder List**: Items with status **To Be Procured**.
2. **Start Work (Production)**: For items made internally, the user begins work and changes the item status from **To Be Produced** to **In Production**.
3. **Start Work (Procurement)**: For items acquired externally, the user places an order with a supplier and changes the item status from **To Be Procured** to **Ordered (Procurement)**.
4. **Order Status Update**: If any order item moves to "In Production" or "Ordered", the parent Order status changes to **Processing**.
5. **Item Completion**: Once the product is made or received from a supplier, the user marks the Order Item as **Ready**.
6. **Order Readiness**: When all Order Items are marked as "Ready" (or were already "Allocated"), the Order status can transition to **Ready to Pack**.

### 5.3 Shipping & Delivery
1. **Packing**: The user marks the order as **Packed** and assigns a Courier (can be edited from confirmation until shipping).
2. **Waybill Generation**: From a list of "Ready to Ship" orders, the user generates a waybill (integrating with the Courier Portal) to get a barcode and tracking number. (Tracking numbers can also be manually entered).
3. **Invoice/Receipt or Shipping bill**: An invoice or receipt can be generated with order status and a shipping bill without courier portal integration for orders and automatically sent with order status message to the customer's WhatsApp number with WhatsApp Business Account integration (Paid service by Meta) at any point.
4. **Dispatch**: Order is handed to the courier; status changes to **Shipped**.
5. **Delivery**: Order reaches the customer; status changes to **Delivered**.

### 5.4 Payments
1. **Payment Method Configuration**: Payment methods (Koko, Payhere) can be configured with fee rates for accurate financial reports

### 5.5 Post-Delivery Payment (COD)
1. For COD orders, the courier service remits the payment after delivery.
2. The user manually adds the payment record to the order in the system.
3. Once the payment status becomes **Fully Paid**, the delivered order automatically or manually transitions to **Completed**.

## 6. Business Rules
- **Confirmation Validation Rule**: An order CANNOT be confirmed if it is NOT marked as "Cash on Delivery" AND has no payments recorded.
- **Stock Flexibility Rule**: An order can be confirmed even if stock is insufficient. The shortage will be tracked as a Fulfillment Task without blocking the order flow.
- **Readiness Rule**: An order cannot transition to "Ready to Pack" unless all its Order Items have a status of "Ready" or "Allocated".
- **Completion Rule**: An order only transitions from "Delivered" to "Completed" if its Payment Status is "Fully Paid".

## 7. Financial Reports
- **Expenses**: Expenses can be added for default or user-defined categories.
- **Financial Report**: A monthly report can be generated to track financial status.

## 8. Integration Points
- **Local Couriers**: Integration for generating waybills, fetching tracking numbers, and potentially updating shipping statuses.
