using DevExpress.DashboardCommon;
using DevExpress.DataAccess.DataFederation;
using DevExpress.XtraEditors;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.Json;
using DevExpress.DataAccess.Sql;
using System;

namespace DataFederationExample {
    public partial class Form1 : XtraForm {
        public Form1() {
            InitializeComponent();
            dashboardDesigner1.CreateRibbon();
            InitializeDashboard();
        }

        public void InitializeDashboard() {
            #region Provide Data Sources            
            Dashboard dashboard = new Dashboard();
            DashboardSqlDataSource sqliteDataSource = CreateSQLiteDataSource();
            dashboard.DataSources.Add(sqliteDataSource);
            DashboardExcelDataSource exceldataSource = CreateExcelDataSource();
            dashboard.DataSources.Add(exceldataSource);
            DashboardObjectDataSource objectDataSource = CreateObjectDataSource();
            dashboard.DataSources.Add(objectDataSource);
            DashboardJsonDataSource jsonDataSource = CreateJsonDataSourceFromFile();
            dashboard.DataSources.Add(jsonDataSource);
            DashboardFederationDataSource federatedDS_Join = CreateFederatedDataSourceJoin(sqliteDataSource, exceldataSource, objectDataSource);
            dashboard.DataSources.Add(federatedDS_Join);
            DashboardFederationDataSource federatedDS_Union = CreateFederatedDataSourceUnion(sqliteDataSource, exceldataSource);
            dashboard.DataSources.Add(federatedDS_Union);
            DashboardFederationDataSource federatedDS_Transform = CreateFederatedDataSourceTransform(jsonDataSource);
            dashboard.DataSources.Add(federatedDS_Transform);
            #endregion

            #region Create a Dashboard
            PivotDashboardItem pivot = new PivotDashboardItem();
            pivot.DataMember = "FDS-Created-by-NodeBulder";
            pivot.DataSource = federatedDS_Join;
            pivot.Rows.AddRange(new Dimension("CategoryName"), new Dimension("ProductName"));
            pivot.Columns.Add(new Dimension("SalesPerson"));
            pivot.Values.Add(new Measure("Extended Price"));

            ChartDashboardItem chart = new ChartDashboardItem();
            chart.DataSource = federatedDS_Join;
            chart.DataMember = "FDS-Created-by-NodeBulder";
            chart.Arguments.Add(new Dimension("SalesPerson"));
            chart.Panes.Add(new ChartPane());
            SimpleSeries theSeries = new SimpleSeries(SimpleSeriesType.Bar);
            theSeries.Value = new Measure("Score");
            chart.Panes[0].Series.Add(theSeries);

            dashboard.Items.AddRange(pivot, chart);
            dashboard.RebuildLayout();
            dashboard.LayoutRoot.Orientation = DashboardLayoutGroupOrientation.Vertical;
            dashboardDesigner1.Dashboard = dashboard;
            #endregion
        }

        private static DashboardFederationDataSource CreateFederatedDataSourceJoin(DashboardSqlDataSource sqliteDataSource, DashboardExcelDataSource exceldataSource, DashboardObjectDataSource objectDataSource) { DashboardFederationDataSource federationDS = new DashboardFederationDataSource("Federated Data Source (JOIN)");
            Source sqlSource = new Source("sqlite", sqliteDataSource, "SQLite Orders");
            Source excelSource = new Source("excel", exceldataSource, "");
            Source objectSource = new Source("object", objectDataSource, "");

            #region Use API to join SQL, Excel, and Object Data Sources in a Query
            SelectNode mainQueryCreatedByApi = new SelectNode();

            mainQueryCreatedByApi.Alias = "FDS-Created-by-API";
            SourceNode sqlSourceNode = new SourceNode(sqlSource, "SQLite Orders");
            SourceNode excelSourceNode = new SourceNode(excelSource, "ExcelDS");
            SourceNode objectSourceNode = new SourceNode(objectSource, "ObjectDS");

            mainQueryCreatedByApi.Root = sqlSourceNode;
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "SalesPerson", Node = objectSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "Weight", Node = objectSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "CategoryName", Node = excelSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "ProductName", Node = excelSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "OrderDate", Node = sqlSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "ShipCity", Node = sqlSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "ShipCountry", Node = sqlSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "Extended Price", Node = excelSourceNode });
            mainQueryCreatedByApi.SubNodes.Add(new JoinElement(excelSourceNode, JoinType.Inner, "[ExcelDS.OrderID] = [SQLite Orders.OrderID]"));
            mainQueryCreatedByApi.SubNodes.Add(new JoinElement(objectSourceNode, JoinType.Inner, "[ObjectDS.SalesPerson] = [ExcelDS.Sales Person]"));
            #endregion

            #region Use NodedBuilder to join SQL, Excel, and Object Data Sources in a Query
            SelectNode mainQueryCreatedByNodeBuilder =
                sqlSource.From()
                .Select("OrderDate", "ShipCity", "ShipCountry")
                .Join(excelSource, "[excel.OrderID] = [sqlite.OrderID]")
                    .Select("CategoryName", "ProductName", "Extended Price")
                    .Join(objectSource, "[object.SalesPerson] = [excel.Sales Person]")
                        .Select("SalesPerson", "Weight")
                        .Build("FDS-Created-by-NodeBulder");
            #endregion

            federationDS.Queries.Add(mainQueryCreatedByApi);
            federationDS.Queries.Add(mainQueryCreatedByNodeBuilder);

            federationDS.CalculatedFields.Add("FDS-Created-by-NodeBulder", "[Weight] * [Extended Price] / 100", "Score");

            federationDS.Fill(new DevExpress.Data.IParameter[0]);
            return federationDS;
        }

        private static DashboardFederationDataSource CreateFederatedDataSourceUnion(DashboardSqlDataSource sqliteDataSource, DashboardExcelDataSource exceldataSource) {
            DashboardFederationDataSource federationDS = new DashboardFederationDataSource("Federated Data Source (UNION)");
            Source sqlSource = new Source("sqlite", sqliteDataSource, "SQLite Orders");
            Source excelSource = new Source("excel", exceldataSource);

            UnionNode queryUnionAll = sqlSource.From().Select("OrderID", "OrderDate").Build("OrdersSqlite")
                .UnionAll(excelSource.From().Select("OrderID", "OrderDate").Build("OrdersExcel"))
                .Build("OrdersUnionAll");

            UnionNode queryUnion = sqlSource.From().Select("OrderID", "OrderDate").Build("OrdersSqlite")
                .Union(excelSource.From().Select("OrderID", "OrderDate").Build("OrdersExcel"))
                .Build("OrdersUnion");


            federationDS.Queries.Add(queryUnionAll);
            federationDS.Queries.Add(queryUnion);

            federationDS.Fill(new DevExpress.Data.IParameter[0]);
            return federationDS;
        }

        private static DashboardFederationDataSource CreateFederatedDataSourceTransform(DashboardJsonDataSource jsonDataSource) {
            DashboardFederationDataSource federationDS = new DashboardFederationDataSource("Federated Data Source (Transformation)");
            Source jsonSource = new Source("json", jsonDataSource, "");
            SourceNode sourceNode = new SourceNode(jsonSource);

            TransformationNode defaultNode = new TransformationNode(sourceNode) {
                Alias = "Default",
                Rules = { new TransformationRule { ColumnName = "Products", Unfold = false, Flatten = false } }
            };

            TransformationNode flattenNode = new TransformationNode(sourceNode) {
                Alias = "Flatten",
                Rules = { new TransformationRule { ColumnName = "Products", Alias = "Product", Unfold = true, Flatten = true } }
            };

            TransformationNode unfoldNode = new TransformationNode(sourceNode) {
                Alias = "Unfold",
                Rules = { new TransformationRule { ColumnName = "Products", Alias = "Product", Unfold = true, Flatten = false } }
            };

            federationDS.Queries.Add(defaultNode);
            federationDS.Queries.Add(flattenNode);
            federationDS.Queries.Add(unfoldNode);
        
            return federationDS;
        }

        private static DashboardObjectDataSource CreateObjectDataSource() {
            DashboardObjectDataSource objectDataSource = new DashboardObjectDataSource("Object Data Source");
            objectDataSource.DataSource = DataGenerator.Data;
            objectDataSource.Fill();
            return objectDataSource;
        }

        private static DashboardSqlDataSource CreateSQLiteDataSource() {
            SQLiteConnectionParameters sqliteParams = new SQLiteConnectionParameters();
            sqliteParams.FileName = @"Data\nwind.db";

            DashboardSqlDataSource sqlDataSource = new DashboardSqlDataSource("SQLite Data Source", sqliteParams);
            SelectQuery selectQuery = SelectQueryFluentBuilder
                .AddTable("Orders")
                .SelectAllColumnsFromTable()
                .Build("SQLite Orders");
            sqlDataSource.Queries.Add(selectQuery);
            sqlDataSource.Fill();
            return sqlDataSource;
        }

        private static DashboardJsonDataSource CreateJsonDataSourceFromFile() {
            var jsonDataSource = new DashboardJsonDataSource("JSON Data Source");
            Uri fileUri = new Uri(@"Data\Categories.json", UriKind.RelativeOrAbsolute);
            jsonDataSource.JsonSource = new UriJsonSource(fileUri);
            jsonDataSource.Fill();
            return jsonDataSource;
        }

        private DashboardExcelDataSource CreateExcelDataSource() {
            DashboardExcelDataSource excelDataSource = new DashboardExcelDataSource("Excel Data Source");
            excelDataSource.FileName = @"Data\SalesPerson.xlsx";
            ExcelWorksheetSettings worksheetSettings = new ExcelWorksheetSettings("Data");
            excelDataSource.SourceOptions = new ExcelSourceOptions(worksheetSettings);
            excelDataSource.Fill();
            return excelDataSource;
        } 
    }
}
