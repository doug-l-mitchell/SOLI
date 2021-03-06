using System.Data;

namespace SoliTests.Repositories
{
    public static class MockDatabase
    {
        public static IDbConnection EstablishDatabase()
        {
            var dbConn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
            dbConn.Open();

            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @"
            create table Invitations
            (
                id blob not null primary key,
                invitor varchar(60) null,
                invitee varchar(60) not null,
                email varchar(60) not null,
                roles varchar(60) not null,
                created datetime default CURRENT_TIMESTAMP
            );

            create table Territories
            (
                id blob primary key,
                name varchar(60) not null,
                color varchar(12) not null,
                state varchar(2) not null
            );

            create table TerritoryZips
            (
                territory_id blob not null,
                zip varchar(24) not null,
                primary key(territory_id, zip),
                foreign key (territory_id) references Territories (id)
            );
            
            create table TerritoryChurches
            (
                territory_id blob not null,
                church varchar(24) not null,
                primary key (territory_id, church),
                foreign key (territory_id) references Territories (id)
            );

            create table Roles
            (
                id integer primary key,
                name varchar(60) not null
            );

            create table Users
            (
                id nvarchar(36) not null primary key,
                displayName varchar(60) not null,
                email varchar(60),
                created datetime default CURRENT_TIMESTAMP
            );

            create table UserClaims
            (
                user_id blob references Users(id),
                role_id integer REFERENCES Roles(id),
                primary key (user_id, role_id)
            );

            -- seed data
            insert into Roles (name) values
            ('soli_territoryadmin'),
            ('soli_inviteuser');
            ";

            cmd.ExecuteNonQuery();
            return dbConn;
        }
    }
}
