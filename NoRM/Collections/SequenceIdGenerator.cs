using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Norm.BSON;

namespace Norm.Collections
{
	/// <summary>
	/// Class that generates a new running number identity value for a collection
	/// </summary>
	public class SequenceIdGenerator
	{
		public static long? Seed { get; set; }

		/// <summary>
		/// Generates a new identity value
		/// </summary>
		/// <param name="collectionName">Collection Name</param>
		/// <param name="database"></param>
		/// <returns></returns>
		public long GenerateId(string collectionName, IMongoDatabase database)
		{
			while (true)
			{
				try
				{
					var update = new Expando();
					update["$inc"] = new {Next = 1};

					var counter = database.GetCollection<SequenceIdCounters>().FindAndModify(new {_id = collectionName}, update);
					
					if (counter == null)
					{
						// account for starting seed
						if (Seed != null)
						{
							database.GetCollection<SequenceIdCounters>()
							.Insert(new SequenceIdCounters
							{
								CollectionName = collectionName,
								Next = Seed.Value+1
							});
							return Seed.Value;
						}

						database.GetCollection<SequenceIdCounters>()
							.Insert(new SequenceIdCounters
							        	{
							        		CollectionName = collectionName,
											Next = 2
							        	});
						return 1;
					}

					var id = counter.Next;
					return id;
				}
				catch (MongoException ex)
				{
					if (!ex.Message.Contains("duplicate key"))
						throw;
				}
			}
		}

		#region Nested type: SequenceIdCounters

		private class SequenceIdCounters
		{
			[MongoIdentifier]
			public string CollectionName { get; set; }

			public long Next { get; set; }
		}

		#endregion
	}
}