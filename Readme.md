<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/185410809/19.2.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T828759)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
<!-- default file list -->
*Files to look at*:

* [Form1.cs](./CS/DataFederationExample/Form1.cs) (VB: [Form1.vb](./VB/DataFederationExample/Form1.vb))
<!-- default file list end -->

# How to Bind a Dashboard to a Federated Data Source Created at Runtime

This example creates at runtime the following data sources:

* [SQL data source connected to the SQLite database](https://docs.devexpress.com/Dashboard/113925)
* [Excel data source](https://docs.devexpress.com/Dashboard/114766)
* [Object data source](https://docs.devexpress.com/Dashboard/16133)

Subsequently the [federated data source](https://docs.devexpress.com/Dashboard/400924) is created to integrate the existing data sources.

This example demonstrates two methods that can be used to create a federated data source in code:

* Data Federation API
* [SelectNodeBuilder](https://docs.devexpress.com/Dashboard/DevExpress.DataAccess.DataFederation.SelectNodeBuilder) 

The application creates a simple dashboard at runtime, binds it to the [DashboardFederationDataSource](https://docs.devexpress.com/Dashboard/DevExpress.DashboardCommon.DashboardFederationDataSource) and displays it in the [DashboardDesigner](https://docs.devexpress.com/Dashboard/DevExpress.DashboardWin.DashboardDesigner) control.


![screenshot](images/screenshot.png)
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=winforms-dashboard-data-federation&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=winforms-dashboard-data-federation&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
