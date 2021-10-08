using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate
{
	class Program
	{
		static void Main(string[] args)
		{
			var configuration = new Configuration();
			configuration.DataBaseIntegration(db =>
			{
				db.ConnectionProvider<DriverConnectionProvider>();
				db.Dialect<MsSql2012Dialect>();
				db.Driver<SqlClientDriver>();
				db.ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=nhtest;Integrated Security=True;Encrypt=False;";
				db.BatchSize = 30;
				db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
				db.Timeout = 10;
				db.LogFormattedSql = true;
				db.LogSqlInConsole = false;
				db.HqlToSqlSubstitutions = "true 1, false 0, yes 'Y', no 'N'";
			});

			var mapper = new ConventionModelMapper();
			var mappings = new[]
			{
				typeof(EtiquetaMap),
				typeof(UsuarioMap),
				typeof(EntradaMap)
			};

			mapper.AddMappings(mappings);
			configuration.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());
			new SchemaExport(configuration).Execute(false, true, false);
		}
	}

	public class EtiquetaMap : ClassMapping<Etiqueta>
	{
		public EtiquetaMap()
		{
			Id(x => x.Id, map => map.Generator(Generators.HighLow, x => x.Params(new { max_low = 100})));
		}
	}

	public class UsuarioMap : ClassMapping<Usuario>
	{
		public UsuarioMap()
		{
			Id(x => x.Id, map => map.Generator(Generators.HighLow, x => x.Params(new { max_low = 100 })));
			Property(x => x.Nombre, map => map.NotNullable(true));
			Property(x => x.Password, map => map.NotNullable(true));
			Bag(p => p.Entradas, map =>
			{
				map.Key(k => k.Column("UsuarioId"));
				map.Table("UsuarioEntradas");
			}, rel => rel.ManyToMany(x => x.Column("EntradaId")));
		}
	}

	public class EntradaMap : ClassMapping<Entrada>
	{
		public EntradaMap()
		{
			Id(x => x.Id, map => map.Generator(Generators.HighLow, x => x.Params(new { max_low = 100 })));
			ManyToOne(x => x.Usuario, map => map.Column("UsuarioId"));
			Bag(x => x.Etiquetas, map =>
			{
				map.Key(k => k.Column("EntradaId"));
				map.Table("EntradaEtiquetas");
			}, rel => rel.ManyToMany(x => x.Column("EtiquetaId")));
		}
	}

	public class Entrada
	{
		public int Id { get; set; }
		public string Titulo { get; set; }
		public string Cuerpo { get; set; }
		public IList<Etiqueta> Etiquetas { get; set; }
		public Usuario Usuario { get; set; }
	}

	public class Usuario
	{
		public int Id { get; set; }
		public string Nombre { get; set; }
		public string Password { get; set; }
		public IList<Entrada> Entradas { get; set; }
	}

	public class Etiqueta
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
