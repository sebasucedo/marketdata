#!/bin/bash
set -e

until mysql -h db -u root -p11235 -e 'select 1'; do
  >&2 echo "MariaDB is unreachable - waiting"
  sleep 1
done

>&2 echo "MariaDB is ready - running migrations"

mysql -h db -u root -p11235 marketdata <<EOF
CREATE TABLE IF NOT EXISTS schema_migrations (
    id INT NOT NULL AUTO_INCREMENT,
    migration VARCHAR(255) NOT NULL,
    executed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) ENGINE=InnoDB;
EOF

for migration in /app/migrations/*.sql; do
  migration_name=$(basename "$migration")
  
  if ! mysql -h db -u root -p11235 marketdata -e "SELECT migration FROM schema_migrations WHERE migration = '$migration_name'" | grep "$migration_name"; then
    >&2 echo "Running migration $migration_name"
    mysql -h db -u root -p11235 marketdata < "$migration"
    
    mysql -h db -u root -p11235 marketdata -e "INSERT INTO schema_migrations (migration) VALUES ('$migration_name')"
  else
    >&2 echo "Migration $migration_name already applied, skipping"
  fi
done

>&2 echo "Migrations applied - starting application"

exec dotnet marketdata.persister.dll
