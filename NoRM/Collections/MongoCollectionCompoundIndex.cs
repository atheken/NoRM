using System;
using System.Linq.Expressions;
using Norm.Protocol.Messages;

namespace Norm.Collections
{
	public class MongoCollectionCompoundIndex<T>
	{
		public MongoCollectionCompoundIndex()
		{
		}

		public MongoCollectionCompoundIndex(Expression<Func<T, object>> index, IndexOption direction)
		{
			Index = index;
			Direction = direction;
		}

		public Expression<Func<T, object>> Index { get; set; }
		public IndexOption Direction { get; set; }
	}
}