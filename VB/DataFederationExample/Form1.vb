Imports Microsoft.VisualBasic
Imports DevExpress.DashboardCommon
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.DataFederation
Imports DevExpress.DataAccess.Excel
Imports DevExpress.DataAccess.Json
Imports DevExpress.DataAccess.Sql
Imports DevExpress.XtraEditors
Imports System

Namespace DataFederationExample
	Partial Public Class Form1
		Inherits XtraForm
		Public Sub New()
			InitializeComponent()
			dashboardDesigner1.CreateRibbon()
			InitializeDashboard()
		End Sub

		Public Sub InitializeDashboard()
			#Region "Provide Data Sources            "
			Dim dashboard As New Dashboard()
			Dim sqliteDataSource As DashboardSqlDataSource = CreateSQLiteDataSource()
			dashboard.DataSources.Add(sqliteDataSource)
			Dim exceldataSource As DashboardExcelDataSource = CreateExcelDataSource()
			dashboard.DataSources.Add(exceldataSource)
			Dim objectDataSource As DashboardObjectDataSource = CreateObjectDataSource()
			dashboard.DataSources.Add(objectDataSource)
			Dim jsonDataSource As DashboardJsonDataSource = CreateJsonDataSourceFromFile()
			dashboard.DataSources.Add(jsonDataSource)
			Dim federatedDS_Join As DashboardFederationDataSource = CreateFederatedDataSourceJoin(sqliteDataSource, exceldataSource, objectDataSource)
			dashboard.DataSources.Add(federatedDS_Join)
			Dim federatedDS_Union As DashboardFederationDataSource = CreateFederatedDataSourceUnion(sqliteDataSource, exceldataSource)
			dashboard.DataSources.Add(federatedDS_Union)
			Dim federatedDS_Transform As DashboardFederationDataSource = CreateFederatedDataSourceTransform(jsonDataSource)
			dashboard.DataSources.Add(federatedDS_Transform)
			#End Region

			#Region "Create a Dashboard"
			Dim pivot As New PivotDashboardItem()
			pivot.DataMember = "FDS-Created-by-NodeBulder"
			pivot.DataSource = federatedDS_Join
			pivot.Rows.AddRange(New Dimension("CategoryName"), New Dimension("ProductName"))
			pivot.Columns.Add(New Dimension("SalesPerson"))
			pivot.Values.Add(New Measure("Extended Price"))

			Dim chart As New ChartDashboardItem()
			chart.DataSource = federatedDS_Join
			chart.DataMember = "FDS-Created-by-NodeBulder"
			chart.Arguments.Add(New Dimension("SalesPerson"))
			chart.Panes.Add(New ChartPane())
			Dim theSeries As New SimpleSeries(SimpleSeriesType.Bar)
			theSeries.Value = New Measure("Score")
			chart.Panes(0).Series.Add(theSeries)

			dashboard.Items.AddRange(pivot, chart)
			dashboard.RebuildLayout()
			dashboard.LayoutRoot.Orientation = DashboardLayoutGroupOrientation.Vertical
			dashboardDesigner1.Dashboard = dashboard
			#End Region
		End Sub

		Private Shared Function CreateFederatedDataSourceJoin(ByVal sqliteDataSource As DashboardSqlDataSource, ByVal exceldataSource As DashboardExcelDataSource, ByVal objectDataSource As DashboardObjectDataSource) As DashboardFederationDataSource
			Dim federationDS As New DashboardFederationDataSource("Federated Data Source (JOIN)")
			Dim sqlSource As New Source("sqlite", sqliteDataSource, "SQLite Orders")
			Dim excelSource As New Source("excel", exceldataSource, "")
			Dim objectSource As New Source("object", objectDataSource, "")

			#Region "Use API to join SQL, Excel, and Object Data Sources in a Query"
			Dim mainQueryCreatedByApi As New SelectNode()

			mainQueryCreatedByApi.Alias = "FDS-Created-by-API"
			Dim sqlSourceNode As New SourceNode(sqlSource, "SQLite Orders")
			Dim excelSourceNode As New SourceNode(excelSource, "ExcelDS")
			Dim objectSourceNode As New SourceNode(objectSource, "ObjectDS")

			mainQueryCreatedByApi.Root = sqlSourceNode
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "SalesPerson", .Node = objectSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "Weight", .Node = objectSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "CategoryName", .Node = excelSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "ProductName", .Node = excelSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "OrderDate", .Node = sqlSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "ShipCity", .Node = sqlSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "ShipCountry", .Node = sqlSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "Extended Price", .Node = excelSourceNode})
			mainQueryCreatedByApi.SubNodes.Add(New JoinElement(excelSourceNode, JoinType.Inner, "[ExcelDS.OrderID] = [SQLite Orders.OrderID]"))
			mainQueryCreatedByApi.SubNodes.Add(New JoinElement(objectSourceNode, JoinType.Inner, "[ObjectDS.SalesPerson] = [ExcelDS.Sales Person]"))
			#End Region

			#Region "Use NodedBuilder to join SQL, Excel, and Object Data Sources in a Query"
			Dim mainQueryCreatedByNodeBuilder As SelectNode = sqlSource.From().Select("OrderDate", "ShipCity", "ShipCountry").Join(excelSource, "[excel.OrderID] = [sqlite.OrderID]").Select("CategoryName", "ProductName", "Extended Price").Join(objectSource, "[object.SalesPerson] = [excel.Sales Person]").Select("SalesPerson", "Weight").Build("FDS-Created-by-NodeBulder")
			#End Region

			federationDS.Queries.Add(mainQueryCreatedByApi)
			federationDS.Queries.Add(mainQueryCreatedByNodeBuilder)

			federationDS.CalculatedFields.Add("FDS-Created-by-NodeBulder", "[Weight] * [Extended Price] / 100", "Score")

			federationDS.Fill(New DevExpress.Data.IParameter(){})
			Return federationDS
		End Function

		Private Shared Function CreateFederatedDataSourceUnion(ByVal sqliteDataSource As DashboardSqlDataSource, ByVal exceldataSource As DashboardExcelDataSource) As DashboardFederationDataSource
			Dim federationDS As New DashboardFederationDataSource("Federated Data Source (UNION)")
			Dim sqlSource As New Source("sqlite", sqliteDataSource, "SQLite Orders")
			Dim excelSource As New Source("excel", exceldataSource)

			Dim queryUnionAll As UnionNode = sqlSource.From().Select("OrderID", "OrderDate").Build("OrdersSqlite").UnionAll(excelSource.From().Select("OrderID", "OrderDate").Build("OrdersExcel")).Build("OrdersUnionAll")

			Dim queryUnion As UnionNode = sqlSource.From().Select("OrderID", "OrderDate").Build("OrdersSqlite").Union(excelSource.From().Select("OrderID", "OrderDate").Build("OrdersExcel")).Build("OrdersUnion")


			federationDS.Queries.Add(queryUnionAll)
			federationDS.Queries.Add(queryUnion)

			federationDS.Fill(New DevExpress.Data.IParameter(){})
			Return federationDS
		End Function

		Private Shared Function CreateFederatedDataSourceTransform(ByVal jsonDataSource As DashboardJsonDataSource) As DashboardFederationDataSource
			Dim federationDS As New DashboardFederationDataSource("Federated Data Source (Transformation)")
			Dim jsonSource As New Source("json", jsonDataSource, "")
			Dim sourceNode As New SourceNode(jsonSource)

			Dim defaultNode As New TransformationNode(sourceNode)
			defaultNode.Alias = "Default"
			defaultNode.Rules.Add(New TransformationRule With {.ColumnName = "Products", .Unfold = False, .Flatten = False})

			Dim flattenNode As New TransformationNode(sourceNode)
			flattenNode.Alias = "Flatten"
			flattenNode.Rules.Add(New TransformationRule With {.ColumnName = "Products", .Alias = "Product", .Unfold = True, .Flatten = True})

			Dim unfoldNode As New TransformationNode(sourceNode)
			unfoldNode.Alias = "Unfold"
			unfoldNode.Rules.Add(New TransformationRule With {.ColumnName = "Products", .Alias = "Product", .Unfold = True, .Flatten = False})

			federationDS.Queries.Add(defaultNode)
			federationDS.Queries.Add(flattenNode)
			federationDS.Queries.Add(unfoldNode)

			Return federationDS
		End Function

		Private Shared Function CreateObjectDataSource() As DashboardObjectDataSource
			Dim objectDataSource As New DashboardObjectDataSource("Object Data Source")
			objectDataSource.DataSource = DataGenerator.Data
			objectDataSource.Fill()
			Return objectDataSource
		End Function

		Private Shared Function CreateSQLiteDataSource() As DashboardSqlDataSource
			Dim sqliteParams As New SQLiteConnectionParameters()
			sqliteParams.FileName = "Data\nwind.db"

			Dim sqlDataSource As New DashboardSqlDataSource("SQLite Data Source", sqliteParams)
			Dim selectQuery As SelectQuery = SelectQueryFluentBuilder.AddTable("Orders").SelectAllColumnsFromTable().Build("SQLite Orders")
			sqlDataSource.Queries.Add(selectQuery)
			sqlDataSource.Fill()
			Return sqlDataSource
		End Function

		Private Shared Function CreateJsonDataSourceFromFile() As DashboardJsonDataSource
			Dim jsonDataSource = New DashboardJsonDataSource("JSON Data Source")
			Dim fileUri As New Uri("Data\Categories.json", UriKind.RelativeOrAbsolute)
			jsonDataSource.JsonSource = New UriJsonSource(fileUri)
			jsonDataSource.Fill()
			Return jsonDataSource
		End Function

		Private Function CreateExcelDataSource() As DashboardExcelDataSource
			Dim excelDataSource As New DashboardExcelDataSource("Excel Data Source")
			excelDataSource.FileName = "Data\SalesPerson.xlsx"
			Dim worksheetSettings As New ExcelWorksheetSettings("Data")
			excelDataSource.SourceOptions = New ExcelSourceOptions(worksheetSettings)
			excelDataSource.Fill()
			Return excelDataSource
		End Function
	End Class
End Namespace
