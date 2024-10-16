<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/185410809/20.2.1%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T828759)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->

# Dashboard for WinForms - How to Bind a Dashboard to a Federated Data Source Created at Runtime

This example creates at runtime the following data sources:

* [SQL data source connected to the SQLite database](https://docs.devexpress.com/Dashboard/113925)
* [Excel data source](https://docs.devexpress.com/Dashboard/114766)
* [Object data source](https://docs.devexpress.com/Dashboard/16133)
* [JSON Data Source](https://docs.devexpress.com/Dashboard/401312)

Subsequently [federated data sources](https://docs.devexpress.com/Dashboard/400924) are created to integrate the existing data sources.

This example demonstrates the following query types you can use to create a data federation:

* **Join**
    
    Combines rows from two or more tables based on a column they share. The join type specifies records that have matching values in both tables.

* **Union and UnionAll**

    **Union** combines rows from two or more tables into a single data set and removes duplicate rows in merged tables. **UnionAll** operates in the same manner as **Union**, but duplicates rows from different tables when they contain the same data. You can create a union query for data sources if data types of their columns are [implicitly converted](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/casting-and-type-conversions#implicit-conversions).        
    
* **Transformation**

    If a data source contains a complex column (an object), you can transform its properties to display them as separate columns in a flattened view. If one of the data column is an array, you can unfold its values and display a new data row for every element of the array. When you unfold the column, you can flatten it and create a flattened view.

The application creates a simple dashboard at runtime, binds it to the [DashboardFederationDataSource](https://docs.devexpress.com/Dashboard/DevExpress.DashboardCommon.DashboardFederationDataSource) created with the Join type and displays the dashboard in the [DashboardDesigner](https://docs.devexpress.com/Dashboard/DevExpress.DashboardWin.DashboardDesigner) control.


![screenshot](images/screenshot.png)

## Files to Review:

* [Form1.cs](./CS/DataFederationExample/Form1.cs) (VB: [Form1.vb](./VB/DataFederationExample/Form1.vb))

## Documentation

- [Federated Data Source](https://docs.devexpress.com/Dashboard/400924)
- [Data Sources](https://docs.devexpress.com/Dashboard/116522)
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=winforms-dashboard-data-federation&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=winforms-dashboard-data-federation&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
