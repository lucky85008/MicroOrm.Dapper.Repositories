﻿using System;
using System.Collections.Generic;
using System.Linq;

using MicroOrm.Dapper.Repositories.SqlGenerator;
using MicroOrm.Dapper.Repositories.Tests.Classes;

using Xunit;

namespace MicroOrm.Dapper.Repositories.Tests.SqlGeneratorTests
{
    public class MsSqlGeneratorTests
    {
        private const SqlProvider _sqlConnector = SqlProvider.MSSQL;

        [Fact]
        public static void Count()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetCount(null);
            Assert.Equal("SELECT COUNT(*) FROM [Users] WHERE [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void CountWithDistinct()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetCount(null, user => user.AddressId);
            Assert.Equal("SELECT COUNT(DISTINCT [Users].[AddressId]) FROM [Users] WHERE [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void CountWithDistinctAndWhere()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetCount(x => x.PhoneId == 1, user => user.AddressId);
            Assert.Equal("SELECT COUNT(DISTINCT [Users].[AddressId]) FROM [Users] WHERE ([Users].[PhoneId] = @PhoneId_p0) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void ChangeDate_Insert()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);

            var user = new User { Name = "Dude" };
            userSqlGenerator.GetInsert(user);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public static void ChangeDate_Update()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);

            var user = new User { Name = "Dude" };
            userSqlGenerator.GetUpdate(user);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public static void ExpressionArgumentException()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);

            var isExceptions = false;

            try
            {
                var sumAr = new List<int> { 1, 2, 3 };
                userSqlGenerator.GetSelectAll(x => sumAr.All(z => x.Id == z));
            }
            catch (NotSupportedException ex)
            {
                Assert.Contains("'All' method is not supported", ex.Message);
                isExceptions = true;
            }

            Assert.True(isExceptions, "Contains no cast exception");
        }

        [Fact]
        public static void BoolFalseEqualNotPredicate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.IsActive != true);

            var parameters = sqlQuery.Param as IDictionary<string, object>;
            Assert.True(Convert.ToBoolean(parameters["IsActive_p0"]));

            Assert.Equal("SELECT TOP 1 [DAB].[Phones].[Id], [DAB].[Phones].[Number], [DAB].[Phones].[IsActive], [DAB].[Phones].[Code] FROM [DAB].[Phones] WHERE [DAB].[Phones].[IsActive] != @IsActive_p0", sqlQuery.GetSql());
        }

        [Fact]
        public static void BoolFalseEqualPredicate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.IsActive == false);

            var parameters = sqlQuery.Param as IDictionary<string, object>;
            Assert.False(Convert.ToBoolean(parameters["IsActive_p0"]));

            Assert.Equal("SELECT TOP 1 [DAB].[Phones].[Id], [DAB].[Phones].[Number], [DAB].[Phones].[IsActive], [DAB].[Phones].[Code] FROM [DAB].[Phones] WHERE [DAB].[Phones].[IsActive] = @IsActive_p0", sqlQuery.GetSql());
        }

        [Fact]
        public static void BoolFalsePredicate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => !x.IsActive);

            var parameters = sqlQuery.Param as IDictionary<string, object>;
            Assert.False(Convert.ToBoolean(parameters["IsActive_p0"]));

            Assert.Equal("SELECT TOP 1 [DAB].[Phones].[Id], [DAB].[Phones].[Number], [DAB].[Phones].[IsActive], [DAB].[Phones].[Code] FROM [DAB].[Phones] WHERE [DAB].[Phones].[IsActive] = @IsActive_p0", sqlQuery.GetSql());
        }

        [Fact]
        public static void BoolTruePredicate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.IsActive);

            var parameters = sqlQuery.Param as IDictionary<string, object>;
            Assert.True(Convert.ToBoolean(parameters["IsActive_p0"]));

            Assert.Equal("SELECT TOP 1 [DAB].[Phones].[Id], [DAB].[Phones].[Number], [DAB].[Phones].[IsActive], [DAB].[Phones].[Code] FROM [DAB].[Phones] WHERE [DAB].[Phones].[IsActive] = @IsActive_p0", sqlQuery.GetSql());
        }

        [Fact]
        public static void BulkInsertMultiple()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetBulkInsert(new List<Address> { new Address(), new Address() });

            Assert.Equal("INSERT INTO [Addresses] ([Street], [CityId]) VALUES (@Street0, @CityId0),(@Street1, @CityId1)", sqlQuery.GetSql());
        }

        [Fact]
        public static void BulkInsertOne()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetBulkInsert(new List<Address> { new Address() });

            Assert.Equal("INSERT INTO [Addresses] ([Street], [CityId]) VALUES (@Street0, @CityId0)", sqlQuery.GetSql());
        }

        [Fact]
        public static void BulkUpdate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var phones = new List<Phone>
            {
                new Phone { Id = 10, IsActive = true, Number = "111" },
                new Phone { Id = 10, IsActive = false, Number = "222" }
            };

            var sqlQuery = userSqlGenerator.GetBulkUpdate(phones);

            Assert.Equal("UPDATE [DAB].[Phones] SET [Number] = @Number0, [IsActive] = @IsActive0 WHERE [Id] = @Id0; " +
                         "UPDATE [DAB].[Phones] SET [Number] = @Number1, [IsActive] = @IsActive1 WHERE [Id] = @Id1", sqlQuery.GetSql());
        }

        [Fact]
        public static void BulkUpdateIgnoreOneOfKeys()
        {
            ISqlGenerator<Report> userSqlGenerator = new SqlGenerator<Report>(_sqlConnector, true);
            var reports = new List<Report>
            {
                new Report { Id = 10, AnotherId = 10, UserId = 22 },
                new Report { Id = 10, AnotherId = 10, UserId = 23 }
            };

            var sqlQuery = userSqlGenerator.GetBulkUpdate(reports);

            Assert.Equal("UPDATE [Reports] SET [UserId] = @UserId0 WHERE [AnotherId] = @AnotherId0; " +
                         "UPDATE [Reports] SET [UserId] = @UserId1 WHERE [AnotherId] = @AnotherId1", sqlQuery.GetSql());
        }

        [Fact]
        public static void ContainsPredicate()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var ids = new List<int>();
            var sqlQuery = userSqlGenerator.GetSelectAll(x => ids.Contains(x.Id));

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] " +
                         "FROM [Users] WHERE ([Users].[Id] IN @Id_p0) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void ContainsArrayPredicate()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var ids = new int[] { };
            var sqlQuery = userSqlGenerator.GetSelectAll(x => ids.Contains(x.Id));

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] " +
                         "FROM [Users] WHERE ([Users].[Id] IN @Id_p0) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void NotContainsPredicate()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var ids = new List<int>();
            var sqlQuery = userSqlGenerator.GetSelectAll(x => !ids.Contains(x.Id));

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] " +
                         "FROM [Users] WHERE ([Users].[Id] NOT IN @Id_p0) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void LogicalDeleteWithUpdatedAt()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector);
            var user = new User() { Id = 10 };
            var sqlQuery = userSqlGenerator.GetDelete(user);
            var sql = sqlQuery.GetSql();

            Assert.Equal("UPDATE Users SET Deleted = 1, UpdatedAt = @UpdatedAt WHERE Users.Id = @Id", sql);
        }

        [Fact]
        public static void LogicalleleteWithUpdatedAtWithPredicate()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector);
            var user = new User() { Id = 10 };
            var sqlQuery = userSqlGenerator.GetDelete(user);
            var sql = sqlQuery.GetSql();

            Assert.Equal("UPDATE Users SET Deleted = 1, UpdatedAt = @UpdatedAt WHERE Users.Id = @Id", sql);
        }

        [Fact]
        public static void Delete()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var phone = new Phone { Id = 10, Code = "ZZZ", IsActive = true, Number = "111" };
            var sqlQuery = userSqlGenerator.GetDelete(phone);

            Assert.Equal("DELETE FROM [DAB].[Phones] WHERE [DAB].[Phones].[Id] = @Id", sqlQuery.GetSql());
        }

        [Fact]
        public static void LogicalDeleteEntity()
        {
            ISqlGenerator<Car> sqlGenerator = new SqlGenerator<Car>(_sqlConnector);
            var car = new Car() { Id = 10, Name = "LogicalDelete", UserId = 5 };

            var sqlQuery = sqlGenerator.GetDelete(car);
            var realSql = sqlQuery.GetSql();
            Assert.Equal("UPDATE Cars SET Status = -1 WHERE Cars.Id = @Id", realSql);
        }

        [Fact]
        public static void LogicalDeletePredicate()
        {
            ISqlGenerator<Car> sqlGenerator = new SqlGenerator<Car>(_sqlConnector);

            var sqlQuery = sqlGenerator.GetDelete(q => q.Id == 10);
            var realSql = sqlQuery.GetSql();

            Assert.Equal("UPDATE Cars SET Status = -1 WHERE Cars.Id = @Id_p0", realSql);
        }

        [Fact]
        public static void DeleteWithMultiplePredicate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetDelete(x => x.IsActive && x.Number == "111");

            Assert.Equal("DELETE FROM [DAB].[Phones] WHERE [DAB].[Phones].[IsActive] = @IsActive_p0 AND [DAB].[Phones].[Number] = @Number_p1", sqlQuery.GetSql());
        }

        [Fact]
        public static void DeleteWithSinglePredicate()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetDelete(x => x.IsActive);

            Assert.Equal("DELETE FROM [DAB].[Phones] WHERE [DAB].[Phones].[IsActive] = @IsActive_p0", sqlQuery.GetSql());
        }

        [Fact]
        public static void InsertQuoMarks()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetInsert(new Address());

            Assert.Equal("INSERT INTO [Addresses] ([Street], [CityId]) VALUES (@Street, @CityId) SELECT SCOPE_IDENTITY() AS [Id]", sqlQuery.GetSql());
        }

        [Fact]
        public static void IsNull()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(user => user.UpdatedAt == null);

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] FROM [Users] " +
                         "WHERE ([Users].[UpdatedAt] IS NULL) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
            Assert.DoesNotContain("== NULL", sqlQuery.GetSql());
        }

        [Fact]
        public static void JoinBracelets()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectAll(null, user => user.Cars);

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt], " +
                         "[Cars_Id].[Id], [Cars_Id].[Name], [Cars_Id].[Data], [Cars_Id].[UserId], [Cars_Id].[Status] " +
                         "FROM [Users] LEFT JOIN [Cars] AS [Cars_Id] ON [Users].[Id] = [Cars_Id].[UserId] " +
                         "WHERE [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void NavigationPredicate()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.Phone.Number == "123", user => user.Phone);

            Assert.Equal("SELECT TOP 1 [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt], " +
                         "[Phones_PhoneId].[Id], [Phones_PhoneId].[Number], [Phones_PhoneId].[IsActive], [Phones_PhoneId].[Code] " +
                         "FROM [Users] INNER JOIN [DAB].[Phones] AS [Phones_PhoneId] ON [Users].[PhoneId] = [Phones_PhoneId].[Id] " +
                         "WHERE ([Phones_PhoneId].[Number] = @PhoneNumber_p0) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void NavigationPredicateNoQuotationMarks()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, false);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.Phone.Number == "123", user => user.Phone);

            Assert.Equal("SELECT TOP 1 Users.Id, Users.Name, Users.AddressId, Users.PhoneId, Users.OfficePhoneId, Users.Deleted, Users.UpdatedAt, " +
                         "Phones_PhoneId.Id, Phones_PhoneId.Number, Phones_PhoneId.IsActive, Phones_PhoneId.Code " +
                         "FROM Users INNER JOIN DAB.Phones AS Phones_PhoneId ON Users.PhoneId = Phones_PhoneId.Id " +
                         "WHERE (Phones_PhoneId.Number = @PhoneNumber_p0) AND Users.Deleted != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void SelectBetweenWithLogicalDelete()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, false);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

            Assert.Equal("SELECT Users.Id, Users.Name, Users.AddressId, Users.PhoneId, Users.OfficePhoneId, Users.Deleted, Users.UpdatedAt FROM Users " +
                         "WHERE Users.Deleted != 1 AND Users.Id BETWEEN '1' AND '10'", sqlQuery.GetSql());
        }

        [Fact]
        public static void SelectBetweenWithLogicalDeleteBraclets()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

            Assert.Equal("SELECT [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] FROM [Users] " +
                         "WHERE [Users].[Deleted] != 1 AND [Users].[Id] BETWEEN '1' AND '10'", sqlQuery.GetSql());
        }

        [Fact]
        public static void SelectBetweenWithoutLogicalDelete()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(_sqlConnector, false);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

            Assert.Equal("SELECT Addresses.Id, Addresses.Street, Addresses.CityId FROM Addresses " +
                         "WHERE Addresses.Id BETWEEN '1' AND '10'", sqlQuery.GetSql());
        }

        [Fact]
        public static void SelectBetweenWithoutLogicalDeleteBraclets()
        {
            ISqlGenerator<Address> userSqlGenerator = new SqlGenerator<Address>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectBetween(1, 10, x => x.Id);

            Assert.Equal("SELECT [Addresses].[Id], [Addresses].[Street], [Addresses].[CityId] FROM [Addresses] " +
                         "WHERE [Addresses].[Id] BETWEEN '1' AND '10'", sqlQuery.GetSql());
        }

        [Fact]
        public static void SelectById()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectById(1);

            Assert.Equal("SELECT TOP 1 [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] " +
                         "FROM [Users] WHERE [Users].[Id] = @Id AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void SelectFirst()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sqlQuery = userSqlGenerator.GetSelectFirst(x => x.Id == 2);
            Assert.Equal("SELECT TOP 1 [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt] FROM [Users] WHERE ([Users].[Id] = @Id_p0) AND [Users].[Deleted] != 1", sqlQuery.GetSql());
        }

        [Fact]
        public static void UpdateExclude()
        {
            ISqlGenerator<Phone> userSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var phone = new Phone { Id = 10, Code = "ZZZ", IsActive = true, Number = "111" };
            var sqlQuery = userSqlGenerator.GetUpdate(phone);

            Assert.Equal("UPDATE [DAB].[Phones] SET [Number] = @Number, [IsActive] = @IsActive WHERE [Id] = @Id", sqlQuery.GetSql());
        }

        [Fact]
        public static void UpdateWithPredicate()
        {
            ISqlGenerator<City> sqlGenerator = new SqlGenerator<City>(_sqlConnector);
            var sqlQuery = sqlGenerator.GetUpdate(q => q.Identifier == Guid.Empty, new City());
            var sql = sqlQuery.GetSql();
            Assert.Equal("UPDATE Cities SET Identifier = @Identifier, Name = @Name WHERE Cities.Identifier = @Identifier_p0", sql);
        }

        #region Support `group conditions` syntax

        [Fact]
        public static void SelectGroupConditionsWithPredicate()
        {
            ISqlGenerator<Phone> phoneSqlGenerator = new SqlGenerator<Phone>(_sqlConnector, true);
            var sPrefix = "SELECT [DAB].[Phones].[Id], [DAB].[Phones].[Number], [DAB].[Phones].[IsActive], [DAB].[Phones].[Code] FROM [DAB].[Phones] WHERE ";

            var sqlQuery1 = phoneSqlGenerator.GetSelectAll(x => (x.IsActive && x.Id == 123) || (x.Id == 456 && x.Number == "456"));
            Assert.Equal(sPrefix + "([DAB].[Phones].[IsActive] = @IsActive_p0 AND [DAB].[Phones].[Id] = @Id_p1) OR ([DAB].[Phones].[Id] = @Id_p2 AND [DAB].[Phones].[Number] = @Number_p3)", sqlQuery1.GetSql());

            var sqlQuery2 = phoneSqlGenerator.GetSelectAll(x => !x.IsActive || (x.Id == 456 && x.Number == "456"));
            Assert.Equal(sPrefix + "[DAB].[Phones].[IsActive] = @IsActive_p0 OR ([DAB].[Phones].[Id] = @Id_p1 AND [DAB].[Phones].[Number] = @Number_p2)", sqlQuery2.GetSql());

            var sqlQuery3 = phoneSqlGenerator.GetSelectAll(x => (x.Id == 456 && x.Number == "456") || x.Id == 123);
            Assert.Equal(sPrefix + "([DAB].[Phones].[Id] = @Id_p0 AND [DAB].[Phones].[Number] = @Number_p1) OR [DAB].[Phones].[Id] = @Id_p2", sqlQuery3.GetSql());

            var sqlQuery4 = phoneSqlGenerator.GetSelectAll(x => x.Number == "1" && (x.IsActive || x.Number == "456") && x.Id == 123);
            Assert.Equal(sPrefix + "[DAB].[Phones].[Number] = @Number_p0 AND ([DAB].[Phones].[IsActive] = @IsActive_p1 OR [DAB].[Phones].[Number] = @Number_p2) AND [DAB].[Phones].[Id] = @Id_p3", sqlQuery4.GetSql());

            var sqlQuery5 = phoneSqlGenerator.GetSelectAll(x => x.Number == "1" && (x.IsActive || x.Number == "456" || x.Number == "678") && x.Id == 123);
            Assert.Equal(sPrefix + "[DAB].[Phones].[Number] = @Number_p0 AND ([DAB].[Phones].[IsActive] = @IsActive_p1 OR [DAB].[Phones].[Number] = @Number_p2 OR [DAB].[Phones].[Number] = @Number_p3) AND [DAB].[Phones].[Id] = @Id_p4", sqlQuery5.GetSql());

            var ids = new List<int>();
            var sqlQuery6 = phoneSqlGenerator.GetSelectAll(x => !x.IsActive || (x.IsActive && ids.Contains(x.Id)));
            Assert.Equal(sPrefix + "[DAB].[Phones].[IsActive] = @IsActive_p0 OR ([DAB].[Phones].[IsActive] = @IsActive_p1 AND [DAB].[Phones].[Id] IN @Id_p2)", sqlQuery6.GetSql());

            var sqlQuery7 = phoneSqlGenerator.GetSelectAll(x => (x.IsActive && x.Id == 123) && (x.Id == 456 && x.Number == "456"));
            Assert.Equal(sPrefix + "[DAB].[Phones].[IsActive] = @IsActive_p0 AND [DAB].[Phones].[Id] = @Id_p1 AND [DAB].[Phones].[Id] = @Id_p2 AND [DAB].[Phones].[Number] = @Number_p3", sqlQuery7.GetSql());

            var sqlQuery8 = phoneSqlGenerator.GetSelectAll(x => x.Number == "1" && (x.IsActive || x.Number == "456" || x.Number == "123" || (x.Id == 1213 && x.Number == "678")) && x.Id == 123);
            Assert.Equal(sPrefix + "[DAB].[Phones].[Number] = @Number_p0 AND ([DAB].[Phones].[IsActive] = @IsActive_p1 OR [DAB].[Phones].[Number] = @Number_p2 OR [DAB].[Phones].[Number] = @Number_p3 OR ([DAB].[Phones].[Id] = @Id_p4 AND [DAB].[Phones].[Number] = @Number_p5)) AND [DAB].[Phones].[Id] = @Id_p6", sqlQuery8.GetSql());

            var sqlQuery9 = phoneSqlGenerator.GetSelectAll(x => (x.Id == 456 && x.Number == "456") && x.Id == 123 && (x.Id == 4567 && x.Number == "4567"));
            Assert.Equal(sPrefix + "[DAB].[Phones].[Id] = @Id_p0 AND [DAB].[Phones].[Number] = @Number_p1 AND [DAB].[Phones].[Id] = @Id_p2 AND [DAB].[Phones].[Id] = @Id_p3 AND [DAB].[Phones].[Number] = @Number_p4", sqlQuery9.GetSql());
        }

        [Fact]
        public static void SelectGroupConditionsNavigationPredicate()
        {
            ISqlGenerator<User> userSqlGenerator = new SqlGenerator<User>(_sqlConnector, true);
            var sPrefix = "SELECT TOP 1 [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt], " +
                        "[Phones_PhoneId].[Id], [Phones_PhoneId].[Number], [Phones_PhoneId].[IsActive], [Phones_PhoneId].[Code] " +
                        "FROM [Users] INNER JOIN [DAB].[Phones] AS [Phones_PhoneId] ON [Users].[PhoneId] = [Phones_PhoneId].[Id] " +
                        "WHERE ";

            var sqlQuery1 = userSqlGenerator.GetSelectFirst(x => x.Phone.Number == "123" || (x.Name == "abc" && x.Phone.IsActive), user => user.Phone);
            Assert.Equal(sPrefix + "([Phones_PhoneId].[Number] = @PhoneNumber_p0 OR ([Users].[Name] = @Name_p1 AND [Phones_PhoneId].[IsActive] = @PhoneIsActive_p2)) AND [Users].[Deleted] != 1", sqlQuery1.GetSql());

            var ids = new List<int>();
            var sqlQuery2 = userSqlGenerator.GetSelectFirst(x => x.Phone.Number != "123" && (x.Name != "abc" || !x.Phone.IsActive || !ids.Contains(x.PhoneId) || !ids.Contains(x.Phone.Id)) && (x.Name == "abc" || x.Phone.IsActive), user => user.Phone);
            Assert.Equal(sPrefix + "([Phones_PhoneId].[Number] != @PhoneNumber_p0 AND ([Users].[Name] != @Name_p1 OR [Phones_PhoneId].[IsActive] = @PhoneIsActive_p2 OR [Users].[PhoneId] NOT IN @PhoneId_p3 OR [Phones_PhoneId].[Id] NOT IN @PhoneId_p4) AND ([Users].[Name] = @Name_p5 OR [Phones_PhoneId].[IsActive] = @PhoneIsActive_p6)) AND [Users].[Deleted] != 1", sqlQuery2.GetSql());
        }

        #endregion

        #region Support `like` syntax

        [Fact]
        public static void SelectLikeWithPredicate()
        {
            ISqlGenerator<Phone> phoneSqlGenerator1 = new SqlGenerator<Phone>(_sqlConnector, true);
            var sPrefix1 = "SELECT [DAB].[Phones].[Id], [DAB].[Phones].[Number], [DAB].[Phones].[IsActive], [DAB].[Phones].[Code] FROM [DAB].[Phones] WHERE ";

            var sqlQuery11 = phoneSqlGenerator1.GetSelectAll(x => x.Code.StartsWith("123", StringComparison.OrdinalIgnoreCase) || !x.Code.EndsWith("456") || x.Code.Contains("789"));
            Assert.Equal(sPrefix1 + "[DAB].[Phones].[Code] LIKE @Code_p0 OR [DAB].[Phones].[Code] NOT LIKE @Code_p1 OR [DAB].[Phones].[Code] LIKE @Code_p2", sqlQuery11.GetSql());

            var parameters11 = sqlQuery11.Param as IDictionary<string, object>;
            Assert.True("123%" == parameters11["Code_p0"].ToString());
            Assert.True("%456" == parameters11["Code_p1"].ToString());
            Assert.True("%789%" == parameters11["Code_p2"].ToString());

            ISqlGenerator<User> userSqlGenerator2 = new SqlGenerator<User>(_sqlConnector, true);
            var sPrefix2 = "SELECT TOP 1 [Users].[Id], [Users].[Name], [Users].[AddressId], [Users].[PhoneId], [Users].[OfficePhoneId], [Users].[Deleted], [Users].[UpdatedAt], " +
            "[Phones_PhoneId].[Id], [Phones_PhoneId].[Number], [Phones_PhoneId].[IsActive], [Phones_PhoneId].[Code] " +
            "FROM [Users] INNER JOIN [DAB].[Phones] AS [Phones_PhoneId] ON [Users].[PhoneId] = [Phones_PhoneId].[Id] " +
            "WHERE ";

            var sqlQuery21 = userSqlGenerator2.GetSelectFirst(x => x.Phone.Number.StartsWith("123") || (!x.Name.Contains("abc") && x.Phone.IsActive), user => user.Phone);
            Assert.Equal(sPrefix2 + "([Phones_PhoneId].[Number] LIKE @PhoneNumber_p0 OR ([Users].[Name] NOT LIKE @Name_p1 AND [Phones_PhoneId].[IsActive] = @PhoneIsActive_p2)) AND [Users].[Deleted] != 1", sqlQuery21.GetSql());
            var parameters21 = sqlQuery21.Param as IDictionary<string, object>;
            Assert.True("123%" == parameters21["PhoneNumber_p0"].ToString());
            Assert.True("%abc%" == parameters21["Name_p1"].ToString());
        }

        #endregion
    }
}
