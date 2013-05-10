////////////////////////////////////////////////////////////////////////////////////////////////////////
// LLBLGen Pro is (c) 2002-2013 Solutions Design. All rights reserved.
// http://www.llblgen.com
////////////////////////////////////////////////////////////////////////////////////////////////////////
// COPYRIGHTS:
// Copyright (c)2002-2013 Solutions Design. All rights reserved.
// 
// This LLBLGen Pro Contrib library is released under the following license: (BSD2)
// ---------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met: 
//
// 1) Redistributions of source code must retain the above copyright notice, this list of 
//    conditions and the following disclaimer. 
// 2) Redistributions in binary form must reproduce the above copyright notice, this list of 
//    conditions and the following disclaimer in the documentation and/or other materials 
//    provided with the distribution. 
//
// THIS SOFTWARE IS PROVIDED BY SOLUTIONS DESIGN ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, 
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL SOLUTIONS DESIGN OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
//
// The views and conclusions contained in the software and documentation are those of the authors 
// and should not be interpreted as representing official policies, either expressed or implied, 
// of Solutions Design.
//////////////////////////////////////////////////////////////////////
// Contributers to the code:
//		- Frans Bouma [FB]
//////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SD.LLBLGen.Pro.ORMSupportClasses.Contrib
{
	/// <summary>
	/// Wrapper class which makes it possible to use LLBLGen Pro Adapter code in an async way. Doesn't make the LLBLGen Pro
	/// runtime framework become Asynchronous, it only allows a caller to perform a non-blocking call using await: all the
	/// work done by the LLBLGen Pro Runtime Framework is still synchronous code. 
	/// </summary>
	/// <typeparam name="TAdapter">The IDataAccessAdapter implementing class to use as DataAccessAdapter.</typeparam>
	/// <remarks>
	/// The AsyncDataAccessAdapter class is stateless; it doesn't store an active DataAccessAdapter
	/// instance. This means that this class can't be used to perform multiple async operations using the
	/// same DataAccessAdapter instance (e.g. to use the same transaction). If your code requires multiple
	/// actions on the same DataAccessAdapter because they have to happen on the same transaction, either
	/// create an async method returning a Task which calls the adapter methods synchronously, or use
	/// a TransactionScope from System.Transactions, or wrap everything in a Unit of Work and commit the
	/// Unit of Work in an asynchronous method returning a Task. The latter can also be done with the
	/// Asynchronous extension methods for unit of work, found elsewhere in this library.
	/// <br /><br />
	/// By default the class creates a new instance of TAdapter for each action, using the connection string
	/// that it would normally use for the adapter. To use a specific connection string, pass it to the
	/// ctor of this class.
	/// <br/><br/>
	/// While the methods in this class are asynchronous, the code called by this class is still synchronous, as
	/// the LLBLGen Pro Runtime Framework itself is not async aware as it has to run on .NET 3.5 as well and
	/// therefore can't use the Task/TPL functionality of .NET 4.0 or higher. This class helps deblock calling
	/// code while it waits for the database code to finish. In the future the runtime will be updated with
	/// a full async core around the DbCommand/DbDataReader classes. 
	/// <br/><br/>
	/// Not all methods available on IDataAccessAdapter are implemented, as some made no sense in an asynchronous
	/// context, like FetchDataReader. No overload calling is done in this class, as it would assume call order/flow
	/// of the wrapped class. 
	/// </remarks>
	public class AsyncAdapterWrapper<TAdapter>
		where TAdapter : class, IDataAccessAdapter, new()
	{
		#region Member declarations
		private string _alternativeConnectionString;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncAdapterWrapper{TAdapter}"/> class. Will use
		/// default connection string defined on TAdapter.
		/// </summary>
		public AsyncAdapterWrapper() : this(string.Empty)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncAdapterWrapper{TAdapter}"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string to use for the async operations.</param>
		public AsyncAdapterWrapper(string connectionString)
		{
			_alternativeConnectionString = connectionString ?? string.Empty;
			this.TransactionIsolationLevel = IsolationLevel.Unspecified;
		}


		/// <summary>
		/// Asynchronous variant of DeleteEntitiesDirectly. 
		/// Deletes all entities of the name passed in as <i>entityName</i> (e.g. "CustomerEntity") from the persistent storage if they match the filter
		/// supplied in <i>filterBucket</i>.
		/// </summary>
		/// <param name="typeOfEntity">The type of the entity to retrieve persistence information. </param>
		/// <param name="filterBucket">filter information to filter out the entities to delete</param>
		/// <returns>the amount of physically deleted entities</returns>
		public async Task<int> DeleteEntitiesDirectlyAsync(Type typeOfEntity, IRelationPredicateBucket filterBucket)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.DeleteEntitiesDirectly(typeOfEntity, filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of DeleteEntitiesDirectly. 
		/// Deletes all entities of the name passed in as <i>entityName</i> (e.g. "CustomerEntity") from the persistent storage if they match the filter
		/// supplied in <i>filterBucket</i>.
		/// </summary>
		/// <param name="entityName">The name of the entity to retrieve persistence information. For example "CustomerEntity". This name can be
		/// retrieved from an existing entity's Name property.</param>
		/// <param name="filterBucket">filter information to filter out the entities to delete</param>
		/// <returns>the amount of physically deleted entities</returns>
		public async Task<int> DeleteEntitiesDirectly(string entityName, IRelationPredicateBucket filterBucket)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.DeleteEntitiesDirectly(entityName, filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of DeleteEntity. 
		/// Deletes the specified entity from the persistent storage. The entity is not usable after this call, the state is set to OutOfSync.
		/// Will use the current transaction if a transaction is in progress.
		/// </summary>
		/// <param name="entityToDelete">The entity instance to delete from the persistent storage</param>
		/// <returns>true if the delete was succesful, otherwise false.</returns>
		public async Task<bool> DeleteEntity(IEntity2 entityToDelete)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.DeleteEntity(entityToDelete);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of DeleteEntity. 
		/// Deletes the specified entity from the persistent storage. The entity is not usable after this call, the state is set to OutOfSync.
		/// Will use the current transaction if a transaction is in progress.
		/// </summary>
		/// <param name="entityToDelete">The entity instance to delete from the persistent storage</param>
		/// <param name="deleteRestriction">Predicate expression, meant for concurrency checks in a delete query</param>
		/// <returns>true if the delete was succesful, otherwise false.</returns>
		public async Task<bool> DeleteEntityAsync(IEntity2 entityToDelete, IPredicateExpression deleteRestriction)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.DeleteEntity(entityToDelete, deleteRestriction);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of DeleteEntityCollection. 
		/// Deletes all dirty objects inside the collection passed from the persistent storage. It will do this inside a transaction if a transaction
		/// is not yet available. Entities which are physically deleted from the persistent storage are marked with the state 'Deleted' but are not
		/// removed from the collection.
		/// If the passed in entity has a concurrency predicate factory object, the returned predicate expression is used to restrict the delete process.		
		/// </summary>
		/// <param name="collectionToDelete">EntityCollection with one or more dirty entities which have to be persisted</param>
		/// <returns>the amount of physically deleted entities</returns>
		public async Task<int> DeleteEntityCollectionAsync(IEntityCollection2 collectionToDelete)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.DeleteEntityCollection(collectionToDelete);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntity. 
		/// Fetches an entity from the persistent storage into the passed in Entity2 object using a primary key filter. The primary key fields of
		/// the entity passed in have to have the primary key values. (Example: CustomerID has to have a value, when you want to fetch a CustomerEntity
		/// from the persistent storage into the passed in object). All fields specified in excludedFields are excluded from the fetch so the entity won't
		/// get any value set for those fields. <b>excludedFields</b> can be null or empty, in which case all fields are fetched (default).
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored. The primary key fields have to have a value.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		/// <remarks>Will use a current transaction if a transaction is in progress, so MVCC or other concurrency scheme used by the database can be utilized</remarks>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityAsync(IEntity2 entityToFetch, IPrefetchPath2 prefetchPath, Context contextToUse, 
											     ExcludeIncludeFieldsList excludedIncludedFields)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntity(entityToFetch, prefetchPath, contextToUse, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntity. 
		/// Fetches an entity from the persistent storage into the passed in Entity2 object using a primary key filter. The primary key fields of
		/// the entity passed in have to have the primary key values. (Example: CustomerID has to have a value, when you want to fetch a CustomerEntity
		/// from the persistent storage into the passed in object)
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored. The primary key fields have to have a value.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <remarks>Will use a current transaction if a transaction is in progress, so MVCC or other concurrency scheme used by the database can be utilized</remarks>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityAsync(IEntity2 entityToFetch, IPrefetchPath2 prefetchPath, Context contextToUse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntity(entityToFetch, prefetchPath, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntity. 
		/// Fetches an entity from the persistent storage into the passed in Entity2 object using a primary key filter. The primary key fields of
		/// the entity passed in have to have the primary key values. (Example: CustomerID has to have a value, when you want to fetch a CustomerEntity
		/// from the persistent storage into the passed in object)
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored. The primary key fields have to have a value.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful. </param>
		/// <remarks>Will use a current transaction if a transaction is in progress, so MVCC or other concurrency scheme used by the database can be
		/// utilized</remarks>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityAsync(IEntity2 entityToFetch, Context contextToUse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntity(entityToFetch, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntity. 
		/// Fetches an entity from the persistent storage into the passed in Entity2 object using a primary key filter. The primary key fields of
		/// the entity passed in have to have the primary key values. (Example: CustomerID has to have a value, when you want to fetch a CustomerEntity
		/// from the persistent storage into the passed in object)
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored. The primary key fields have to have a value.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <remarks>Will use a current transaction if a transaction is in progress, so MVCC or other concurrency scheme used by the database can be
		/// utilized</remarks>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityAsync(IEntity2 entityToFetch, IPrefetchPath2 prefetchPath)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntity(entityToFetch, prefetchPath);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntity. 
		/// Fetches an entity from the persistent storage into the passed in Entity2 object using a primary key filter. The primary key fields of
		/// the entity passed in have to have the primary key values. (Example: CustomerID has to have a value, when you want to fetch a CustomerEntity
		/// from the persistent storage into the passed in object)
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored. The primary key fields have to have a value.</param>
		/// <remarks>Will use a current transaction if a transaction is in progress, so MVCC or other concurrency scheme used by the database can be
		/// utilized</remarks>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityAsync(IEntity2 entityToFetch)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntity(entityToFetch);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// It will apply paging and it will from there use a prefetch path fetch using the read page. It's important that pageSize
		/// is smaller than the set <see cref="ParameterisedPrefetchPathThreshold"/>. If pagesize is larger than the limits set for
		/// the <see cref="ParameterisedPrefetchPathThreshold"/> value, the query is likely to be slower than expected, though will work.
		/// If pageNumber / pageSize are set to values which disable paging, a normal prefetch path fetch will be performed. 
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="prefetchPath">Prefetch path to use.</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		/// <param name="pageNumber">the page number to retrieve. First page is 1. When set to 0, no paging logic is applied</param>
		/// <param name="pageSize">the size of the page. When set to 0, no paging logic is applied</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
													  ISortExpression sortClauses, IPrefetchPath2 prefetchPath, ExcludeIncludeFieldsList excludedIncludedFields, 
													  int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, prefetchPath, excludedIncludedFields, 
												  pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// It will apply paging and it will from there use a prefetch path fetch using the read page. It's important that pageSize
		/// is smaller than the set ParameterisedPrefetchPathThreshold. It will work, though if pagesize is larger than the limits set for
		/// the ParameterisedPrefetchPathThreshold value, the query is likely to be slower than expected.
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="prefetchPath">Prefetch path to use.</param>
		/// <param name="pageNumber">the page number to retrieve. First page is 1. When set to 0, no paging logic is applied</param>
		/// <param name="pageSize">the size of the page. When set to 0, no paging logic is applied</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
													  ISortExpression sortClauses, IPrefetchPath2 prefetchPath, int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, prefetchPath, pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="pageNumber">the page number to retrieve. First page is 1. When set to 0, no paging logic is applied</param>
		/// <param name="pageSize">the size of the page. When set to 0, no paging logic is applied</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
													  ISortExpression sortClauses, int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
													  ISortExpression sortClauses, IPrefetchPath2 prefetchPath, ExcludeIncludeFieldsList excludedIncludedFields)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, prefetchPath, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
													  ISortExpression sortClauses, IPrefetchPath2 prefetchPath)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, prefetchPath);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <exception cref="ArgumentException">If the passed in collectionToFill doesn't contain an entity factory.</exception>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
													 ISortExpression sortClauses)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// This overload doesn't apply sorting
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of entities to return. If 0, all entities matching the filter are returned</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, maxNumberOfItemsToReturn);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// This overload returns all found entities and doesn't apply sorting
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket, prefetchPath);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// This overload returns all found entities and doesn't apply sorting
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, ExcludeIncludeFieldsList excludedIncludedFields, 
													 IRelationPredicateBucket filterBucket)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, excludedIncludedFields, filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the filterBucket into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// This overload returns all found entities and doesn't apply sorting
		/// </summary>
		/// <param name="collectionToFill">EntityCollection object containing an entity factory which has to be filled</param>
		/// <param name="filterBucket">filter information for retrieving the entities. If null, all entities are returned of the type created by
		/// the factory in the passed in EntityCollection instance.</param>
		public async Task FetchEntityCollectionAsync(IEntityCollection2 collectionToFill, IRelationPredicateBucket filterBucket)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(collectionToFill, filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityCollection. 
		/// Fetches one or more entities which match the filter information in the parameters into the EntityCollection passed.
		/// The entity collection object has to contain an entity factory object which will be the factory for the entity instances
		/// to be fetched.
		/// It will apply paging and it will from there use a prefetch path fetch using the read page. It's important that pageSize
		/// is smaller than the set <see cref="ParameterisedPrefetchPathThreshold" />. If pagesize is larger than the limits set for
		/// the <see cref="ParameterisedPrefetchPathThreshold" /> value, the query is likely to be slower than expected, though will work.
		/// If pageNumber / pageSize are set to values which disable paging, a normal prefetch path fetch will be performed.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		public async Task FetchEntityCollectionAsync(QueryParameters parameters)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchEntityCollection(parameters);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityUsingUniqueConstraint. 
		/// Fetches an entity from the persistent storage into the object specified using the filter specified. 
		/// Use the entity's uniqueconstraint filter construction methods to construct the required uniqueConstraintFilter for the 
		/// unique constraint you want to use.
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored.</param>
		/// <param name="uniqueConstraintFilter">The filter which should filter on fields with a unique constraint.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityUsingUniqueConstraintAsync(IEntity2 entityToFetch, IPredicateExpression uniqueConstraintFilter, 
																	  IPrefetchPath2 prefetchPath, Context contextToUse, 
																	  ExcludeIncludeFieldsList excludedIncludedFields)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntityUsingUniqueConstraint(entityToFetch, uniqueConstraintFilter, prefetchPath, contextToUse, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityUsingUniqueConstraint. 
		/// Fetches an entity from the persistent storage into the object specified using the filter specified. 
		/// Use the entity's uniqueconstraint filter construction methods to construct the required uniqueConstraintFilter for the 
		/// unique constraint you want to use.
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored.</param>
		/// <param name="uniqueConstraintFilter">The filter which should filter on fields with a unique constraint.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityUsingUniqueConstraintAsync(IEntity2 entityToFetch, IPredicateExpression uniqueConstraintFilter, 
																	  IPrefetchPath2 prefetchPath, Context contextToUse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntityUsingUniqueConstraint(entityToFetch, uniqueConstraintFilter, prefetchPath, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityUsingUniqueConstraint. 
		/// Fetches an entity from the persistent storage into the object specified using the filter specified. 
		/// Use the entity's uniqueconstraint filter construction methods to construct the required uniqueConstraintFilter for the 
		/// unique constraint you want to use.
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored.</param>
		/// <param name="uniqueConstraintFilter">The filter which should filter on fields with a unique constraint.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful. </param>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityUsingUniqueConstraintAsync(IEntity2 entityToFetch, IPredicateExpression uniqueConstraintFilter, Context contextToUse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntityUsingUniqueConstraint(entityToFetch, uniqueConstraintFilter, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityUsingUniqueConstraint. 
		/// Fetches an entity from the persistent storage into the object specified using the filter specified. 
		/// Use the entity's uniqueconstraint filter construction methods to construct the required uniqueConstraintFilter for the 
		/// unique constraint you want to use.
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored.</param>
		/// <param name="uniqueConstraintFilter">The filter which should filter on fields with a unique constraint.</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityUsingUniqueConstraintAsync(IEntity2 entityToFetch, IPredicateExpression uniqueConstraintFilter, 
																	  IPrefetchPath2 prefetchPath)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntityUsingUniqueConstraint(entityToFetch, uniqueConstraintFilter, prefetchPath);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchEntityUsingUniqueConstraint. 
		/// Fetches an entity from the persistent storage into the object specified using the filter specified. 
		/// Use the entity's uniqueconstraint filter construction methods to construct the required uniqueConstraintFilter for the 
		/// unique constraint you want to use.
		/// </summary>
		/// <param name="entityToFetch">The entity object in which the fetched entity data will be stored.</param>
		/// <param name="uniqueConstraintFilter">The filter which should filter on fields with a unique constraint.</param>
		/// <returns>true if the Fetch was succesful, false otherwise</returns>
		public async Task<bool> FetchEntityUsingUniqueConstraintAsync(IEntity2 entityToFetch, IPredicateExpression uniqueConstraintFilter)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchEntityUsingUniqueConstraint(entityToFetch, uniqueConstraintFilter);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchExcludedFields. 
		/// Loads the data for the excluded fields specified in the list of excluded fields into all the entities in the entities collection passed in.
		/// </summary>
		/// <param name="entities">The entities to load the excluded field data into. The entities have to be either of the same type or have to be 
		/// in the same inheritance hierarchy as the entity which factory is set in the collection.</param>
		/// <param name="excludedIncludedFields">The excludedIncludedFields object as it is used when fetching the passed in collection. If you used 
		/// the excludedIncludedFields object to fetch only the fields in that list (i.e. excludedIncludedFields.ExcludeContainedFields==false), the routine
		/// will fetch all other fields in the resultset for the entities in the collection excluding the fields in excludedIncludedFields.</param>
		/// <remarks>The field data is set like a normal field value set, so authorization is applied to it.
		/// This routine batches fetches to have at most 5*ParameterisedPrefetchPathThreshold of parameters per fetch. Keep in mind that most databases have a limit
		/// on the # of parameters per query. 
		/// </remarks>
		public async Task FetchExcludedFieldsAsync(IEntityCollection2 entities, ExcludeIncludeFieldsList excludedIncludedFields)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchExcludedFields(entities, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchExcludedFields. 
		/// Loads the data for the excluded fields specified in the list of excluded fields into the entity passed in. 
		/// </summary>
		/// <param name="entity">The entity to load the excluded field data into.</param>
		/// <param name="excludedIncludedFields">The excludedIncludedFields object as it is used when fetching the passed in entity. If you used 
		/// the excludedIncludedFields object to fetch only the fields in that list (i.e. excludedIncludedFields.ExcludeContainedFields==false), the routine
		/// will fetch all other fields in the resultset for the entities in the collection excluding the fields in excludedIncludedFields.</param>
		/// <remarks>The field data is set like a normal field value set, so authorization is applied to it.</remarks>
		public async Task FetchExcludedFields(IEntity2 entity, ExcludeIncludeFieldsList excludedIncludedFields)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchExcludedFields(entity, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity(Of TEntity). 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// specified generic type. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		/// <returns>The new entity fetched, or a previous entity fetched if that entity was in the context specified</returns>
		/// <typeparam name="TEntity">The type of entity to fetch</typeparam>
		/// <remarks>TEntity can't be a type which is an abstract entity. If you want to fetch an instance of an abstract entity (e.g. polymorphic fetch)
		/// please use the overload which accepts an entity factory instead</remarks>
		public async Task<TEntity> FetchNewEntityAsync<TEntity>(IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath, Context contextToUse, 
																 ExcludeIncludeFieldsList excludedIncludedFields) 
			where TEntity : EntityBase2, IEntity2, new()
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity<TEntity>(filterBucket, prefetchPath, contextToUse, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity(Of TEntity). 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// specified generic type. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <returns>The new entity fetched, or a previous entity fetched if that entity was in the context specified</returns>
		/// <typeparam name="TEntity">The type of entity to fetch</typeparam>
		/// <remarks>TEntity can't be a type which is an abstract entity. If you want to fetch an instance of an abstract entity (e.g. polymorphic fetch)
		/// please use the overload which accepts an entity factory instead</remarks>
		public async Task<TEntity> FetchNewEntityAsync<TEntity>(IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath, Context contextToUse) 
			where TEntity : EntityBase2, IEntity2, new()
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity<TEntity>(filterBucket, prefetchPath, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity(Of TEntity). 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// specified generic type. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <returns>The new entity fetched.</returns>
		/// <typeparam name="TEntity">The type of entity to fetch</typeparam>
		/// <remarks>TEntity can't be a type which is an abstract entity. If you want to fetch an instance of an abstract entity (e.g. polymorphic fetch)
		/// please use the overload which accepts an entity factory instead</remarks>
		public async Task<TEntity> FetchNewEntityAsync<TEntity>(IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath) 
			where TEntity : EntityBase2, IEntity2, new()
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity<TEntity>(filterBucket, prefetchPath);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity(Of TEntity). 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// specified generic type. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful. </param>
		/// <returns>The new entity fetched, or a previous entity fetched if that entity was in the context specified</returns>
		/// <typeparam name="TEntity">The type of entity to fetch</typeparam>
		/// <remarks>TEntity can't be a type which is an abstract entity. If you want to fetch an instance of an abstract entity (e.g. polymorphic fetch)
		/// please use the overload which accepts an entity factory instead</remarks>
		public async Task<TEntity> FetchNewEntityAsync<TEntity>(IRelationPredicateBucket filterBucket, Context contextToUse) 
			where TEntity : EntityBase2, IEntity2, new()
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity<TEntity>(filterBucket, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity(Of TEntity). 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// specified generic type. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <returns>The new entity fetched.</returns>
		/// <typeparam name="TEntity">The type of entity to fetch</typeparam>
		/// <remarks>TEntity can't be a type which is an abstract entity. If you want to fetch an instance of an abstract entity (e.g. polymorphic fetch)
		/// please use the overload which accepts an entity factory instead</remarks>
		public async Task<TEntity> FetchNewEntityAsync<TEntity>(IRelationPredicateBucket filterBucket) 
			where TEntity : EntityBase2, IEntity2, new()
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity<TEntity>(filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity. 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// passed in entity factory. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="entityFactoryToUse">The factory which will be used to create a new entity object which will be fetched</param>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <param name="excludedIncludedFields">The list of IEntityField2 objects which have to be excluded or included for the fetch. 
		/// If null or empty, all fields are fetched (default). If an instance of ExcludeIncludeFieldsList is passed in and its ExcludeContainedFields property
		/// is set to false, the fields contained in excludedIncludedFields are kept in the query, the rest of the fields in the query are excluded.</param>
		/// <returns>The new entity fetched, or a previous entity fetched if that entity was in the context specified</returns>
		public async Task<IEntity2> FetchNewEntityAsync(IEntityFactory2 entityFactoryToUse, IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath, 
														 Context contextToUse, ExcludeIncludeFieldsList excludedIncludedFields)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity(entityFactoryToUse, filterBucket, prefetchPath, contextToUse, excludedIncludedFields);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity. 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// passed in entity factory. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="entityFactoryToUse">The factory which will be used to create a new entity object which will be fetched</param>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful, and before the prefetch path is fetched. This ensures
		/// that the prefetch path is fetched using the context specified and will re-use already loaded entity objects.</param>
		/// <returns>The new entity fetched, or a previous entity fetched if that entity was in the context specified</returns>
		public async Task<IEntity2> FetchNewEntityAsync(IEntityFactory2 entityFactoryToUse, IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath, 
														Context contextToUse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity(entityFactoryToUse, filterBucket, prefetchPath, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity. 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// passed in entity factory. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="entityFactoryToUse">The factory which will be used to create a new entity object which will be fetched</param>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="contextToUse">The context to add the entity to if the fetch was succesful. </param>
		/// <returns>The new entity fetched, or a previous entity fetched if that entity was in the context specified</returns>
		public async Task<IEntity2> FetchNewEntityAsync(IEntityFactory2 entityFactoryToUse, IRelationPredicateBucket filterBucket, Context contextToUse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity(entityFactoryToUse, filterBucket, contextToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity. 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// passed in entity factory. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="entityFactoryToUse">The factory which will be used to create a new entity object which will be fetched</param>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <param name="prefetchPath">The prefetch path to use for this fetch, which will fetch all related entities defined by the path as well.</param>
		/// <returns>The new entity fetched.</returns>
		public async Task<IEntity2> FetchNewEntityAsync(IEntityFactory2 entityFactoryToUse, IRelationPredicateBucket filterBucket, IPrefetchPath2 prefetchPath)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity(entityFactoryToUse, filterBucket, prefetchPath);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchNewEntity. 
		/// Fetches a new entity using the filter/relation combination filter passed in via <i>filterBucket</i> and the new entity is created using the
		/// passed in entity factory. Use this method when fetching a related entity using a current entity (for example, fetch the related Customer entity
		/// of an existing Order entity)
		/// </summary>
		/// <param name="entityFactoryToUse">The factory which will be used to create a new entity object which will be fetched</param>
		/// <param name="filterBucket">the completely filled in IRelationPredicateBucket object which will be used as a filter for the fetch. The fetch
		/// will only load the first entity loaded, even if the filter results into more entities being fetched</param>
		/// <returns>The new entity fetched.</returns>
		public async Task<IEntity2> FetchNewEntityAsync(IEntityFactory2 entityFactoryToUse, IRelationPredicateBucket filterBucket)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.FetchNewEntity(entityFactoryToUse, filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchProjection. 
		/// Creates a new Retrieval query from the elements passed in, executes that retrievalquery and projects the resultset of that query using the
		/// value projectors and the projector specified. If a transaction is in progress, the command is wired to the transaction and executed inside the
		/// transaction. The projection results will be stored in the projector.
		/// </summary>
		/// <param name="valueProjectors">The value projectors.</param>
		/// <param name="projector">The projector to use for projecting a raw row onto a new object provided by the projector.</param>
		/// <param name="parameters">The parameters.</param>
		public async Task FetchProjectionAsync(List<IDataValueProjector> valueProjectors, IGeneralDataProjector projector, QueryParameters parameters)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchProjection(valueProjectors, projector, parameters);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchProjection. 
		/// Executes the passed in retrievalquery and projects the resultset using the value projectors and the projector specified.
		/// The projection results will be stored in the projector.
		/// </summary>
		/// <param name="valueProjectors">The value projectors.</param>
		/// <param name="projector">The projector to use for projecting a raw row onto a new object provided by the projector.</param>
		/// <param name="queryToExecute">The query to execute.</param>
		public async Task FetchProjectionAsync(List<IDataValueProjector> valueProjectors, IGeneralDataProjector projector, IRetrievalQuery queryToExecute)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchProjection(valueProjectors, projector, queryToExecute);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchProjection. 
		/// Creates a new Retrieval query from the elements passed in, executes that retrievalquery and projects the resultset of that query using the
		/// value projectors and the projector specified. If a transaction is in progress, the command is wired to the transaction and executed inside the
		/// transaction. The projection results will be stored in the projector.
		/// </summary>
		/// <param name="valueProjectors">The value projectors.</param>
		/// <param name="projector">The projector to use for projecting a raw row onto a new object provided by the projector.</param>
		/// <param name="fields">The fields to use to build the query.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="maxNumberOfItemsToReturn">The max number of items to return. Specify 0 to return all elements</param>
		/// <param name="sortClauses">The sort clauses.</param>
		/// <param name="groupByClause">The group by clause.</param>
		/// <param name="allowDuplicates">If set to true, allow duplicates in the resultset, otherwise it will emit DISTINCT into the query (if possible).</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="pageSize">Size of the page.</param>
		public async Task FetchProjectionAsync(List<IDataValueProjector> valueProjectors, IGeneralDataProjector projector, IEntityFields2 fields, 
											   IRelationPredicateBucket filter, int maxNumberOfItemsToReturn, ISortExpression sortClauses, 
											   IGroupByCollection groupByClause, bool allowDuplicates, int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchProjection(valueProjectors, projector, fields, filter, maxNumberOfItemsToReturn, sortClauses, groupByClause, allowDuplicates, 
											pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchProjection. 
		/// Creates a new Retrieval query from the elements passed in, executes that retrievalquery and projects the resultset of that query using the
		/// value projectors and the projector specified. If a transaction is in progress, the command is wired to the transaction and executed inside the
		/// transaction. The projection results will be stored in the projector.
		/// </summary>
		/// <param name="valueProjectors">The value projectors.</param>
		/// <param name="projector">The projector to use for projecting a raw row onto a new object provided by the projector.</param>
		/// <param name="fields">The fields to use to build the query.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="maxNumberOfItemsToReturn">The max number of items to return. Specify 0 to return all elements</param>
		/// <param name="sortClauses">The sort clauses.</param>
		/// <param name="allowDuplicates">If set to true, allow duplicates in the resultset, otherwise it will emit DISTINCT into the query (if possible).</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="pageSize">Size of the page.</param>
		public async Task FetchProjectionAsync(List<IDataValueProjector> valueProjectors, IGeneralDataProjector projector, IEntityFields2 fields, 
											   IRelationPredicateBucket filter, int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates, 
											   int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchProjection(valueProjectors, projector, fields, filter, maxNumberOfItemsToReturn, sortClauses, allowDuplicates,
											pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchProjection. 
		/// Creates a new Retrieval query from the elements passed in, executes that retrievalquery and projects the resultset of that query using the
		/// value projectors and the projector specified. If a transaction is in progress, the command is wired to the transaction and executed inside the
		/// transaction. The projection results will be stored in the projector.
		/// </summary>
		/// <param name="valueProjectors">The value projectors.</param>
		/// <param name="projector">The projector to use for projecting a raw row onto a new object provided by the projector.</param>
		/// <param name="fields">The fields to use to build the query.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="maxNumberOfItemsToReturn">The max number of items to return. Specify 0 to return all elements</param>
		/// <param name="sortClauses">The sort clauses.</param>
		/// <param name="allowDuplicates">If set to true, allow duplicates in the resultset, otherwise it will emit DISTINCT into the query (if possible).</param>
		public async Task FetchProjectionAsync(List<IDataValueProjector> valueProjectors, IGeneralDataProjector projector, IEntityFields2 fields, 
											   IRelationPredicateBucket filter, int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchProjection(valueProjectors, projector, fields, filter, maxNumberOfItemsToReturn, sortClauses, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchProjection. 
		/// Creates a new Retrieval query from the elements passed in, executes that retrievalquery and projects the resultset of that query using the
		/// value projectors and the projector specified. If a transaction is in progress, the command is wired to the transaction and executed inside the
		/// transaction. The projection results will be stored in the projector.
		/// </summary>
		/// <param name="valueProjectors">The value projectors.</param>
		/// <param name="projector">The projector to use for projecting a raw row onto a new object provided by the projector.</param>
		/// <param name="fields">The fields to use to build the query.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="maxNumberOfItemsToReturn">The max number of items to return. Specify 0 to return all elements</param>
		/// <param name="allowDuplicates">If set to true, allow duplicates in the resultset, otherwise it will emit DISTINCT into the query (if possible).</param>
		public async Task FetchProjectionAsync(List<IDataValueProjector> valueProjectors, IGeneralDataProjector projector, IEntityFields2 fields, 
											   IRelationPredicateBucket filter, int maxNumberOfItemsToReturn, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchProjection(valueProjectors, projector, fields, filter, maxNumberOfItemsToReturn, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// the passed in typed list.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="typedListToFill">Typed list to fill.</param>
		/// <param name="additionalFilter">An additional filter to use to filter the fetch of the typed list. If you need a more sophisticated
		/// filter approach, please use the overload which accepts an IRelationalPredicateBucket and add your filter to the bucket you receive
		/// when calling typedListToFill.GetRelationInfo().</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <param name="pageNumber">the page number to retrieve. First page is 1. When set to 0, no paging logic is applied</param>
		/// <param name="pageSize">the size of the page. When set to 0, no paging logic is applied</param>
		/// <remarks>Grabs the fields list and relations set from the typed list passed in. </remarks>
		public async Task FetchTypedListAsync(ITypedListLgp2 typedListToFill, IPredicateExpression additionalFilter, int maxNumberOfItemsToReturn, 
											  ISortExpression sortClauses, bool allowDuplicates, int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(typedListToFill, additionalFilter, maxNumberOfItemsToReturn, sortClauses, allowDuplicates, pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// the passed in typed list.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="typedListToFill">Typed list to fill.</param>
		/// <param name="additionalFilter">An additional filter to use to filter the fetch of the typed list. If you need a more sophisticated
		/// filter approach, please use the overload which accepts an IRelationalPredicateBucket and add your filter to the bucket you receive
		/// when calling typedListToFill.GetRelationInfo().</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <remarks>Grabs the fields list and relations set from the typed list passed in. </remarks>
		public async Task FetchTypedListAsync(ITypedListLgp2 typedListToFill, IPredicateExpression additionalFilter, int maxNumberOfItemsToReturn, 
											  ISortExpression sortClauses, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(typedListToFill, additionalFilter, maxNumberOfItemsToReturn, sortClauses, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// the passed in typed list.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="typedListToFill">Typed list to fill.</param>
		/// <param name="additionalFilter">An additional filter to use to filter the fetch of the typed list. If you need a more sophisticated
		/// filter approach, please use the overload which accepts an IRelationalPredicateBucket and add your filter to the bucket you receive
		/// when calling typedListToFill.GetRelationInfo().</param>
		/// <remarks>Grabs the fields list and relations set from the typed list passed in. </remarks>
		public async Task FetchTypedListAsync(ITypedListLgp2 typedListToFill, IPredicateExpression additionalFilter)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(typedListToFill, additionalFilter);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// the passed in typed list.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="typedListToFill">Typed list to fill.</param>
		/// <remarks>Grabs the fields list and relations set from the typed list passed in. </remarks>
		public async Task FetchTypedListAsync(ITypedListLgp2 typedListToFill)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(typedListToFill);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="parameters">The parameters.</param>
		public async Task FetchTypedListAsync(DataTable dataTableToFill, QueryParameters parameters)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(dataTableToFill, parameters);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object. 
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields to fetch into the datatable. The fields
		/// inside this object are used to construct the selection resultset. Use the typed list's method GetFieldsInfo() to retrieve
		/// this IEntityField2 information</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <param name="groupByClause">GroupByCollection with fields to group by on</param>
		/// <param name="pageNumber">the page number to retrieve. First page is 1. When set to 0, no paging logic is applied</param>
		/// <param name="pageSize">the size of the page. When set to 0, no paging logic is applied</param>
		public async Task FetchTypedListAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates, IGroupByCollection groupByClause, 
											  int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates, 
											groupByClause, pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object. 
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields to fetch into the datatable. The fields
		/// inside this object are used to construct the selection resultset. Use the typed list's method GetFieldsInfo() to retrieve
		/// this IEntityField2 information</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <param name="groupByClause">GroupByCollection with fields to group by on</param>
		public async Task FetchTypedListAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates, IGroupByCollection groupByClause)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates,
											groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object. 
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields to fetch into the datatable. The fields
		/// inside this object are used to construct the selection resultset. Use the typed list's method GetFieldsInfo() to retrieve
		/// this IEntityField2 information</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedListAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
										      int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object. Doesn't apply any sorting.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields to fetch into the datatable. The fields
		/// inside this object are used to construct the selection resultset.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedListAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  int maxNumberOfItemsToReturn, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object. Doesn't apply any sorting, doesn't limit
		/// the resultset on the amount of rows to return.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields to fetch into the datatable. The fields
		/// inside this object are used to construct the selection resultset.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedListAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(fieldCollectionToFetch, dataTableToFill, filterBucket, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedList. 
		/// Fetches the fields passed in fieldCollectionToFetch from the persistent storage using the relations and filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a typed list object. Doesn't apply any sorting, doesn't limit
		/// the resultset on the amount of rows to return, does allow duplicates.
		/// For TypedView filling, use the method FetchTypedView()
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields to fetch into the datatable. The fields
		/// inside this object are used to construct the selection resultset.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		public async Task FetchTypedListAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedList(fieldCollectionToFetch, dataTableToFill, filterBucket);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View passed in from the persistent storage 
		/// Doesn't apply any sorting, doesn't limit the amount of rows returned by the query, doesn't apply any filtering.
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="typedViewToFill">Typed view to fill.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View passed in from the persistent storage 
		/// Doesn't apply any sorting. Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="typedViewToFill">Typed view to fill.</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
											  bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill, filterBucket, maxNumberOfItemsToReturn, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View passed in from the persistent storage 
		/// </summary>
		/// <param name="typedViewToFill">Typed view to fill.</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <param name="groupByClause">GroupByCollection with fields to group by on</param>
		/// <remarks>To fill a DataTable with entity rows, use this method as well, by passing the Fields collection of an entity of the same type
		/// as the one you want to fetch as fieldCollectionToFetch.</remarks>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
											  ISortExpression sortClauses, bool allowDuplicates, IGroupByCollection groupByClause)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates, groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View passed in from the persistent storage 
		/// </summary>
		/// <param name="typedViewToFill">Typed view to fill.</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <remarks>To fill a DataTable with entity rows, use this method as well, by passing the Fields collection of an entity of the same type
		/// as the one you want to fetch as fieldCollectionToFetch.</remarks>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill, IRelationPredicateBucket filterBucket, int maxNumberOfItemsToReturn, 
											  ISortExpression sortClauses, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View passed in from the persistent storage 
		/// Doesn't apply any sorting, doesn't limit the amount of rows returned by the query.
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="typedViewToFill">Typed view to fill.</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill, IRelationPredicateBucket filterBucket, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill, filterBucket, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View passed in from the persistent storage
		/// Doesn't apply any sorting, doesn't limit the amount of rows returned by the query, doesn't apply any filtering, allows duplicate rows.
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="typedViewToFill">Typed view to fill.</param>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage using the query information stored in
		/// parameters into the DataTable object passed in. Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="dataTableToFill">The data table to fill.</param>
		/// <param name="parameters">The parameters.</param>
		public async Task FetchTypedViewAsync(DataTable dataTableToFill, QueryParameters parameters)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(dataTableToFill, parameters);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the typed view, using the query specified. 
		/// </summary>
		/// <param name="typedViewToFill">The typed view to fill.</param>
		/// <param name="queryToUse">The query to use.</param>
		/// <remarks>Used with stored procedure calling IRetrievalQuery instances to fill a typed view mapped onto a resultset. Be sure
		/// to call Dispose() on the passed in query, as it's not disposed in this method.</remarks>
		public async Task FetchTypedViewAsync(ITypedView2 typedViewToFill, IRetrievalQuery queryToUse)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(typedViewToFill, queryToUse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage into the DataTable object passed in. Doesn't apply any sorting, doesn't limit the amount of rows returned by the query, doesn't
		/// apply any filtering.
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage using the filter information stored in
		/// filterBucket into the DataTable object passed in. Doesn't apply any sorting
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
			                                  int maxNumberOfItemsToReturn, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage using the filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.
		/// Use the Typed View's method GetFieldsInfo() to get this IEntityField2 field collection</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <param name="groupByClause">GroupByCollection with fields to group by on</param>
		/// <param name="pageNumber">the page number to retrieve. First page is 1. When set to 0, no paging logic is applied</param>
		/// <param name="pageSize">the size of the page. When set to 0, no paging logic is applied</param>
		/// <remarks>To fill a DataTable with entity rows, use this method as well, by passing the Fields collection of an entity of the same type
		/// as the one you want to fetch as fieldCollectionToFetch.</remarks>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates, IGroupByCollection groupByClause, 
											  int pageNumber, int pageSize)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates, 
											groupByClause, pageNumber, pageSize);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage using the filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.
		/// Use the Typed View's method GetFieldsInfo() to get this IEntityField2 field collection</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <param name="groupByClause">GroupByCollection with fields to group by on</param>
		/// <remarks>To fill a DataTable with entity rows, use this method as well, by passing the Fields collection of an entity of the same type
		/// as the one you want to fetch as fieldCollectionToFetch.</remarks>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates, IGroupByCollection groupByClause)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates, groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage using the filter information stored in
		/// filterBucket into the DataTable object passed in. Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.
		/// Use the Typed View's method GetFieldsInfo() to get this IEntityField2 field collection</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data.</param>
		/// <param name="maxNumberOfItemsToReturn">The maximum amount of rows to return. If 0, all rows matching the filter are returned</param>
		/// <param name="sortClauses">SortClause expression which is applied to the query executed, sorting the fetch result.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <remarks>To fill a DataTable with entity rows, use this method as well, by passing the Fields collection of an entity of the same type
		/// as the one you want to fetch as fieldCollectionToFetch.</remarks>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  int maxNumberOfItemsToReturn, ISortExpression sortClauses, bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill, filterBucket, maxNumberOfItemsToReturn, sortClauses, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage using the filter information stored in
		/// filterBucket into the DataTable object passed in. Doesn't apply any sorting, doesn't limit the amount of rows returned by the query.
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		/// <param name="filterBucket">filter information (relations and predicate expressions) for retrieving the data. 
		/// Use the TypedList's method GetRelationInfo() to retrieve this bucket.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill, IRelationPredicateBucket filterBucket, 
											  bool allowDuplicates)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill, filterBucket, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of FetchTypedView. 
		/// Fetches the Typed View fields passed in fieldCollectionToFetch from the persistent storage into the DataTable object passed in. Doesn't apply any sorting, doesn't limit the amount of rows returned by the query, doesn't
		/// apply any filtering, allows duplicate rows.
		/// Use this routine to fill a TypedView object.
		/// </summary>
		/// <param name="fieldCollectionToFetch">IEntityField2 collection which contains the fields of the typed view to fetch into the datatable.</param>
		/// <param name="dataTableToFill">The datatable object to fill with the data for the fields in fieldCollectionToFetch</param>
		public async Task FetchTypedViewAsync(IEntityFields2 fieldCollectionToFetch, DataTable dataTableToFill)
		{
			await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					adapter.FetchTypedView(fieldCollectionToFetch, dataTableToFill);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetDbCount. 
		/// Gets the number of rows returned by a query for the fields specified, using the filter and groupby clause specified. 
		/// </summary>
		/// <param name="fields">IEntityFields2 instance with the fields returned by the query to get the rowcount for</param>
		/// <param name="filter">filter to use by the query to get the rowcount for</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <param name="allowDuplicates">When true, it will not filter out duplicate rows, otherwise it will DISTINCT duplicate rows.</param>
		/// <returns>the number of rows the query for the fields specified, using the filter, relations and groupbyClause specified.</returns>
		public async Task<int> GetDbCountAsync(IEntityFields2 fields, IRelationPredicateBucket filter, IGroupByCollection groupByClause, bool allowDuplicates)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetDbCount(fields, filter, groupByClause, allowDuplicates);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetDbCount. 
		/// Gets the number of rows returned by a query for the fields specified, using the filter and groupby clause specified. 
		/// </summary>
		/// <param name="fields">IEntityFields2 instance with the fields returned by the query to get the rowcount for</param>
		/// <param name="filter">filter to use by the query to get the rowcount for</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <returns>the number of rows the query for the fields specified, using the filter, relations and groupbyClause specified.</returns>
		public async Task<int> GetDbCountAsync(IEntityFields2 fields, IRelationPredicateBucket filter, IGroupByCollection groupByClause)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetDbCount(fields, filter, groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetDbCount. 
		/// Gets the number of rows returned by a query for the fields specified, using the filter and groupby clause specified. 
		/// </summary>
		/// <param name="fields">IEntityFields2 instance with the fields returned by the query to get the rowcount for</param>
		/// <param name="filter">filter to use by the query to get the rowcount for</param>
		/// <returns>the number of rows the query for the fields specified, using the filter, relations and groupbyClause specified.</returns>
		public async Task<int> GetDbCountAsync(IEntityFields2 fields, IRelationPredicateBucket filter)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetDbCount(fields, filter);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetDbCount. 
		/// Gets the estimated number of objects returned by a query for objects to store in the entity collection passed in, using the filter and 
		/// groupby clause specified. The number is estimated as duplicate objects can be present in the raw query results, but will be filtered out
		/// when the query result is transformed into objects.
		/// </summary>
		/// <param name="collection">EntityCollection instance which will be fetched by the query to get the rowcount for</param>
		/// <param name="filter">filter to use by the query to get the rowcount for</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <returns>the number of rows the query for the fields specified, using the filter, relations and groupbyClause specified.</returns>
		public async Task<int> GetDbCountAsync(IEntityCollection2 collection, IRelationPredicateBucket filter, IGroupByCollection groupByClause)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetDbCount(collection, filter, groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetDbCount. 
		/// Gets the estimated number of objects returned by a query for objects to store in the entity collection passed in, using the filter and 
		/// groupby clause specified. The number is estimated as duplicate objects can be present in the raw query results, but will be filtered out
		/// when the query result is transformed into objects.
		/// </summary>
		/// <param name="collection">EntityCollection instance which will be fetched by the query to get the rowcount for</param>
		/// <param name="filter">filter to use by the query to get the rowcount for</param>
		/// <returns>the number of rows the query for the fields specified, using the filter, relations and groupbyClause specified.</returns>
		public async Task<int> GetDbCountAsync(IEntityCollection2 collection, IRelationPredicateBucket filter)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetDbCount(collection, filter);
				}
			});
		}
		

		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Executes the expression defined with the field in the fields collection specified, using the various elements defined. The expression is executed as a
		/// scalar query and a single value is returned.
		/// </summary>
		/// <param name="fields">IEntityFields2 instance with a single field with an expression defined and eventually aggregates</param>
		/// <param name="filter">filter to use</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <param name="relations">The relations part of the filter.</param>
		/// <returns>the value which is the result of the expression defined on the specified field</returns>
		public async Task<object> GetScalarAsync(IEntityFields2 fields, IPredicate filter, IGroupByCollection groupByClause, IRelationCollection relations)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(fields, filter, groupByClause, relations);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Executes the expression defined with the field in the fields collection specified, using the various elements defined. The expression is executed as a
		/// scalar query and a single value is returned.
		/// </summary>
		/// <param name="fields">IEntityFields2 instance with a single field with an expression defined and eventually aggregates</param>
		/// <param name="filter">filter to use</param>
		/// <param name="groupByClause">The list of fields to group by on. When not specified or an empty collection is specified, no group by clause
		/// is added to the query. A check is performed for each field in the selectList. If a field in the selectList is not present in the groupByClause
		/// collection, an exception is thrown.</param>
		/// <returns>the value which is the result of the expression defined on the specified field</returns>
		public async Task<object> GetScalarAsync(IEntityFields2 fields, IPredicate filter, IGroupByCollection groupByClause)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(fields, filter, groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Gets a scalar value, calculated with the aggregate and expression specified. the field specified is the field the expression and aggregate are
		/// applied on.
		/// </summary>
		/// <param name="field">Field to which to apply the aggregate function and expression</param>
		/// <param name="expressionToExecute">The expression to execute. Can be null</param>
		/// <param name="aggregateToApply">Aggregate function to apply. </param>
		/// <param name="filter">The filter to apply to retrieve the scalar</param>
		/// <param name="groupByClause">The groupby clause to apply to retrieve the scalar</param>
		/// <param name="relations">The relations part of the filter.</param>
		/// <returns>the scalar value requested</returns>
		public async Task<object> GetScalarAsync(IEntityField2 field, IExpression expressionToExecute, AggregateFunction aggregateToApply, IPredicate filter, 
												 IGroupByCollection groupByClause, IRelationCollection relations)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(field, expressionToExecute, aggregateToApply, filter, groupByClause, relations);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Gets a scalar value, calculated with the aggregate and expression specified. the field specified is the field the expression and aggregate are
		/// applied on.
		/// </summary>
		/// <param name="field">Field to which to apply the aggregate function and expression</param>
		/// <param name="expressionToExecute">The expression to execute. Can be null</param>
		/// <param name="aggregateToApply">Aggregate function to apply. </param>
		/// <param name="filter">The filter to apply to retrieve the scalar</param>
		/// <param name="groupByClause">The groupby clause to apply to retrieve the scalar</param>
		/// <returns>the scalar value requested</returns>
		public async Task<object> GetScalarAsync(IEntityField2 field, IExpression expressionToExecute, AggregateFunction aggregateToApply, IPredicate filter, 
												 IGroupByCollection groupByClause)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(field, expressionToExecute, aggregateToApply, filter, groupByClause);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Gets a scalar value, calculated with the aggregate and expression specified. the field specified is the field the expression and aggregate are
		/// applied on.
		/// </summary>
		/// <param name="field">Field to which to apply the aggregate function and expression</param>
		/// <param name="expressionToExecute">The expression to execute. Can be null</param>
		/// <param name="aggregateToApply">Aggregate function to apply. </param>
		/// <param name="filter">The filter to apply to retrieve the scalar</param>
		/// <returns>the scalar value requested</returns>
		public async Task<object> GetScalarAsync(IEntityField2 field, IExpression expressionToExecute, AggregateFunction aggregateToApply, IPredicate filter)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(field, expressionToExecute, aggregateToApply, filter);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Gets a scalar value, calculated with the aggregate and expression specified. the field specified is the field the expression and aggregate are
		/// applied on.
		/// </summary>
		/// <param name="field">Field to which to apply the aggregate function and expression</param>
		/// <param name="expressionToExecute">The expression to execute. Can be null</param>
		/// <param name="aggregateToApply">Aggregate function to apply. </param>
		/// <returns>the scalar value requested</returns>
		public async Task<object> GetScalarAsync(IEntityField2 field, IExpression expressionToExecute, AggregateFunction aggregateToApply)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(field, expressionToExecute, aggregateToApply);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of GetScalar. 
		/// Gets a scalar value, calculated with the aggregate and expression specified. the field specified is the field the expression and aggregate are
		/// applied on.
		/// </summary>
		/// <param name="field">Field to which to apply the aggregate function and expression</param>
		/// <param name="aggregateToApply">Aggregate function to apply. </param>
		/// <returns>the scalar value requested</returns>
		public async Task<object> GetScalarAsync(IEntityField2 field, AggregateFunction aggregateToApply)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.GetScalar(field, aggregateToApply);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntity. 
		/// Saves the passed in entity to the persistent storage. If the entity is new, it will be inserted, if the entity is existent, the changed
		/// entity fields will be changed in the database. 
		/// Will pass the concurrency predicate returned by GetConcurrencyPredicate(ConcurrencyPredicateType.Save) as update restriction.
		/// </summary>
		/// <param name="entityToSave">The entity to save</param>
		/// <param name="refetchAfterSave">When true, it will refetch the entity from the persistent storage so it will be up-to-date
		/// after the save action.</param>
		/// <param name="recurse">When true, it will save all dirty objects referenced (directly or indirectly) by entityToSave also.</param>
		/// <returns>true if the save was succesful, false otherwise.</returns>
		public async Task<bool> SaveEntityAsync(IEntity2 entityToSave, bool refetchAfterSave, bool recurse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntity(entityToSave, refetchAfterSave, recurse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntity. 
		/// Saves the passed in entity to the persistent storage. If the entity is new, it will be inserted, if the entity is existent, the changed
		/// entity fields will be changed in the database.
		/// </summary>
		/// <param name="entityToSave">The entity to save</param>
		/// <param name="refetchAfterSave">When true, it will refetch the entity from the persistent storage so it will be up-to-date
		/// after the save action.</param>
		/// <param name="updateRestriction">Predicate expression, meant for concurrency checks in an Update query. Will be ignored when the entity is new.</param>
		/// <param name="recurse">When true, it will save all dirty objects referenced (directly or indirectly) by entityToSave also.</param>
		/// <returns>true if the save was succesful, false otherwise.</returns>
		public async Task<bool> SaveEntityAsync(IEntity2 entityToSave, bool refetchAfterSave, IPredicateExpression updateRestriction, bool recurse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntity(entityToSave, refetchAfterSave, updateRestriction, recurse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntity. 
		/// Saves the passed in entity to the persistent storage. If the entity is new, it will be inserted, if the entity is existent, the changed
		/// entity fields will be changed in the database. Will do a recursive save.
		/// Will pass the concurrency predicate returned by GetConcurrencyPredicate(ConcurrencyPredicateType.Save) as update restriction.
		/// </summary>
		/// <param name="entityToSave">The entity to save</param>
		/// <param name="refetchAfterSave">When true, it will refetch the entity from the persistent storage so it will be up-to-date
		/// after the save action.</param>
		/// <param name="updateRestriction">Predicate expression, meant for concurrency checks in an Update query. Will be ignored when the entity is new.</param>
		/// <returns>true if the save was succesful, false otherwise.</returns>
		public async Task<bool> SaveEntityAsync(IEntity2 entityToSave, bool refetchAfterSave, IPredicateExpression updateRestriction)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntity(entityToSave, refetchAfterSave, updateRestriction);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntity. 
		/// Saves the passed in entity to the persistent storage. If the entity is new, it will be inserted, if the entity is existent, the changed
		/// entity fields will be changed in the database. Will do a recursive save.
		/// Will pass the concurrency predicate returned by GetConcurrencyPredicate(ConcurrencyPredicateType.Save) as update restriction.
		/// </summary>
		/// <param name="entityToSave">The entity to save</param>
		/// <param name="refetchAfterSave">When true, it will refetch the entity from the persistent storage so it will be up-to-date
		/// after the save action.</param>
		/// <returns>true if the save was succesful, false otherwise.</returns>
		public async Task<bool> SaveEntityAsync(IEntity2 entityToSave, bool refetchAfterSave)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntity(entityToSave, refetchAfterSave);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntity. 
		/// Saves the passed in entity to the persistent storage. Will <i>not</i> refetch the entity after this save.
		/// The entity will stay out-of-sync. If the entity is new, it will be inserted, if the entity is existent, the changed
		/// entity fields will be changed in the database. Will do a recursive save.
		/// </summary>
		/// <param name="entityToSave">The entity to save</param>
		/// <returns>true if the save was succesful, false otherwise.</returns>
		public async Task<bool> SaveEntityAsync(IEntity2 entityToSave)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntity(entityToSave);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntityCollection. 
		/// Saves all dirty objects inside the collection passed to the persistent storage. It will do this inside a transaction if a transaction
		/// is not yet available.
		/// </summary>
		/// <param name="collectionToSave">EntityCollection with one or more dirty entities which have to be persisted</param>
		/// <param name="refetchSavedEntitiesAfterSave">Refetches a saved entity from the database, so the entity will not be 'out of sync'</param>
		/// <param name="recurse">When true, it will save all dirty objects referenced (directly or indirectly) by the entities inside collectionToSave also.</param>
		/// <returns>the amount of persisted entities</returns>
		public async Task<int> SaveEntityCollectionAsync(IEntityCollection2 collectionToSave, bool refetchSavedEntitiesAfterSave, bool recurse)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntityCollection(collectionToSave, refetchSavedEntitiesAfterSave, recurse);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of SaveEntityCollection . 
		/// Saves all dirty objects inside the collection passed to the persistent storage. It will do this inside a transaction if a transaction
		/// is not yet available. Will not refetch saved entities and will not recursively save the entities.
		/// </summary>
		/// <param name="collectionToSave">EntityCollection with one or more dirty entities which have to be persisted</param>
		/// <returns>the amount of persisted entities</returns>
		public async Task<int> SaveEntityCollectionAsync(IEntityCollection2 collectionToSave)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.SaveEntityCollection(collectionToSave);
				}
			});
		}


		/// <summary>
		/// Asynchronous variant of UpdateEntitiesDirectly. 
		/// Updates all entities of the same type of the entity <i>entityWithNewValues</i> directly in the persistent storage if they match the filter
		/// supplied in <i>filterBucket</i>. Only the fields changed in entityWithNewValues are updated for these fields. 
		/// </summary>
		/// <param name="entityWithNewValues">Entity object which contains the new values for the entities of the same type and which match the filter
		/// in filterBucket. Only fields which are changed are updated.</param>
		/// <param name="filterBucket">filter information to filter out the entities to update.</param>
		/// <returns>the amount of physically updated entities</returns>
		public async Task<int> UpdateEntitiesDirectlyAsync(IEntity2 entityWithNewValues, IRelationPredicateBucket filterBucket)
		{
			return await Task.Run(() =>
			{
				using(var adapter = CreateAdapterInstance())
				{
					return adapter.UpdateEntitiesDirectly(entityWithNewValues, filterBucket);
				}
			});
		}


		/// <summary>
		/// Creates the adapter instance to use and sets the connection string to the alternative set in the Ctor in this class, if any.
		/// </summary>
		/// <returns></returns>
		protected virtual TAdapter CreateAdapterInstance()
		{
			var toReturn = new TAdapter();
			if(!string.IsNullOrEmpty(_alternativeConnectionString))
			{
				toReturn.ConnectionString = _alternativeConnectionString;
			}
			if(this.CommandTimeOut > 0)
			{
				toReturn.CommandTimeOut = this.CommandTimeOut;
			}
			if(this.ParameterisedPrefetchPathThreshold > 0)
			{
				toReturn.ParameterisedPrefetchPathThreshold = this.ParameterisedPrefetchPathThreshold;
			}
			if(this.TransactionIsolationLevel != IsolationLevel.Unspecified)
			{
				toReturn.TransactionIsolationLevel = this.TransactionIsolationLevel;
			}
			return toReturn;
		}


		#region Property declarations
		/// <summary>
		/// Gets or sets the command time out to use. If set to value higher than 0, it is used on the adapter created by this class.
		/// </summary>
		public int CommandTimeOut { get; set;}
		/// <summary>
		/// Gets or sets the parameterised prefetch path threshold to use. If set to a value higher than 0, it is used on the adapter created by this class.
		/// </summary>
		public int ParameterisedPrefetchPathThreshold { get; set; }
		/// <summary>
		/// Gets or sets the default transaction isolation level. If set to a value other than Unspecified, it is used on the
		/// adapter created by this class. 
		/// </summary>
		public IsolationLevel TransactionIsolationLevel { get; set; }
		#endregion
	}
}
