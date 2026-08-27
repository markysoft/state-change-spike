#!/bin/bash

# Calls SQLCMD to verify that system and user databases return "0" which means all databases are in an "online" state,
# then run the configuration script (setup.sql)
# https://docs.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-databases-transact-sql?view=sql-server-2017 

TRIES=60
DB_STATUS=1
i=0

while [[ $DB_STATUS -ne 0 ]] && [[ $i -lt $TRIES ]]; do
	i=$((i+1))
	DB_STATUS=$($MSSQL_SQLCMD_PATH -h -1 -t 1 -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SET NOCOUNT ON; Select COALESCE(SUM(state), 0) from sys.databases") || DB_STATUS=1
	echo "Waiting for database to be ready..."
	sleep 1s
done

if [[ $DB_STATUS -ne 0 ]]; then 
	echo "SQL Server took more than $TRIES seconds to start up or one or more databases are not in an ONLINE state"
	exit 1
fi

# Run the setup script to create the DB and the schema in the DB
echo "Running configuration script..."

$MSSQL_SQLCMD_PATH -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -d master -i /setup-scripts/setup.sql

echo "Configuration completed."