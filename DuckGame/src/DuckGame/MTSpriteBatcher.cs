using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

/// <summary>
/// This class handles the queueing of batch items into the GPU by creating the triangle tesselations
/// that are used to draw the sprite textures. This class supports int.MaxValue number of sprites to be
/// batched and will process them into short.MaxValue groups (strided by 6 for the number of vertices
/// sent to the GPU). 
/// </summary>
internal class MTSpriteBatcher
{
	/// <summary>
	/// Initialization size for the batch item list and queue.
	/// </summary>
	private const int InitialBatchSize = 256;

	/// <summary>
	/// The maximum number of batch items that can be processed per iteration
	/// </summary>
	private const int MaxBatchSize = 5461;

	/// <summary>
	/// Initialization size for the vertex array, in batch units.
	/// </summary>
	private const int InitialVertexArraySize = 256;

	/// <summary>
	/// The list of batch items to process.
	/// </summary>
	private readonly List<MTSpriteBatchItem> _batchItemList;

	private readonly List<MTSimpleSpriteBatchItem> _simpleBatchItemList;

	private readonly List<GeometryItem> _geometryBatch;

	private readonly List<GeometryItemTexture> _geometryBatchTextured;

	/// <summary>
	/// The available MTSpriteBatchItem queue so that we reuse these objects when we can.
	/// </summary>
	private readonly Queue<MTSpriteBatchItem> _freeBatchItemQueue;

	private readonly Queue<MTSimpleSpriteBatchItem> _freeSimpleBatchItemQueue;

	private readonly Queue<GeometryItem> _freeGeometryBatch;

	private readonly Queue<GeometryItemTexture> _freeGeometryBatchTextured;

	/// <summary>
	/// The target graphics device.
	/// </summary>
	private readonly GraphicsDevice _device;

	/// <summary>
	/// Vertex index array. The values in this array never change.
	/// </summary>
	private short[] _index;

	private short[] _simpleIndex;

	private short[] _geometryIndex;

	private short[] _texturedGeometryIndex;

	private VertexPositionColorTexture[] _vertexArray;

	private VertexPositionColor[] _simpleVertexArray;

	private VertexPositionColor[] _geometryVertexArray;

	private VertexPositionColorTexture[] _geometryVertexArrayTextured;

	private MTSpriteBatch _batch;

	private Comparison<MTSpriteBatchItem> CompareTexture = CompareTextureFunc;

	private static Comparison<MTSpriteBatchItem> CompareDepth = CompareDepthFunc;

	private static Comparison<MTSimpleSpriteBatchItem> CompareSimpleDepth = CompareSimpleDepthFunc;

	private static Comparison<GeometryItem> CompareGeometryDepth = CompareGeometryDepthFunc;

	private static Comparison<GeometryItemTexture> CompareTexturedGeometryDepth = CompareTexturedGeometryDepthFunc;

	private static Comparison<MTSpriteBatchItem> CompareReverseDepth = CompareReverseDepthFunc;

	private static Comparison<MTSimpleSpriteBatchItem> CompareSimpleReverseDepth = CompareSimpleReverseDepthFunc;

	private static Comparison<GeometryItem> CompareGeometryReverseDepth = CompareGeometryReverseDepthFunc;

	private static Comparison<GeometryItemTexture> CompareTexturedGeometryReverseDepth = CompareTexturedGeometryReverseDepthFunc;

	public bool hasSimpleItems => _simpleBatchItemList.Count != 0;

	public bool hasGeometryItems => _geometryBatch.Count != 0;

	public bool hasTexturedGeometryItems => _geometryBatchTextured.Count != 0;

	public MTSpriteBatcher(GraphicsDevice device, MTSpriteBatch batch)
	{
		_device = device;
		_batch = batch;
		_batchItemList = new List<MTSpriteBatchItem>(256);
		_freeBatchItemQueue = new Queue<MTSpriteBatchItem>(256);
		_simpleBatchItemList = new List<MTSimpleSpriteBatchItem>(256);
		_freeSimpleBatchItemQueue = new Queue<MTSimpleSpriteBatchItem>(256);
		_geometryBatch = new List<GeometryItem>(1);
		_freeGeometryBatch = new Queue<GeometryItem>(1);
		_geometryBatchTextured = new List<GeometryItemTexture>(1);
		_freeGeometryBatchTextured = new Queue<GeometryItemTexture>(1);
		EnsureArrayCapacity(256);
		EnsureSimpleArrayCapacity(256);
		EnsureGeometryArrayCapacity(256);
		EnsureTexturedGeometryArrayCapacity(256);
	}

	/// <summary>
	/// Create an instance of MTSpriteBatchItem if there is none available in the free item queue. Otherwise,
	/// a previously allocated MTSpriteBatchItem is reused.
	/// </summary>
	/// <returns></returns>
	public MTSpriteBatchItem CreateBatchItem()
	{
		MTSpriteBatchItem item = ((_freeBatchItemQueue.Count <= 0) ? new MTSpriteBatchItem() : _freeBatchItemQueue.Dequeue());
		_batchItemList.Add(item);
		return item;
	}

	public MTSpriteBatchItem StealLastBatchItem()
	{
		MTSpriteBatchItem mTSpriteBatchItem = _batchItemList[_batchItemList.Count - 1];
		mTSpriteBatchItem.inPool = false;
		return mTSpriteBatchItem;
	}

	public void SqueezeInItem(MTSpriteBatchItem item)
	{
		_batchItemList.Add(item);
	}

	public MTSimpleSpriteBatchItem CreateSimpleBatchItem()
	{
		MTSimpleSpriteBatchItem item = ((_freeSimpleBatchItemQueue.Count <= 0) ? new MTSimpleSpriteBatchItem() : _freeSimpleBatchItemQueue.Dequeue());
		_simpleBatchItemList.Add(item);
		return item;
	}

	public static GeometryItem CreateGeometryItem()
	{
		return new GeometryItem
		{
			temporary = false
		};
	}

	public GeometryItem GetGeometryItem()
	{
		GeometryItem item;
		if (_freeGeometryBatch.Count > 0)
		{
			item = _freeGeometryBatch.Dequeue();
			item.material = null;
		}
		else
		{
			item = new GeometryItem
			{
				temporary = true
			};
		}
		item.Clear();
		return item;
	}

	public void SubmitGeometryItem(GeometryItem item)
	{
		if (!_geometryBatch.Contains(item))
		{
			_geometryBatch.Add(item);
		}
	}

	public static GeometryItemTexture CreateTexturedGeometryItem()
	{
		return new GeometryItemTexture
		{
			temporary = false
		};
	}

	public GeometryItemTexture GetTexturedGeometryItem()
	{
		GeometryItemTexture item = ((_freeGeometryBatch.Count <= 0) ? new GeometryItemTexture
		{
			temporary = true
		} : _freeGeometryBatchTextured.Dequeue());
		item.Clear();
		return item;
	}

	public void SubmitTexturedGeometryItem(GeometryItemTexture item)
	{
		if (!_geometryBatchTextured.Contains(item))
		{
			_geometryBatchTextured.Add(item);
		}
	}

	/// <summary>
	/// Resize and recreate the missing indices for the index and vertex position color buffers.
	/// </summary>
	/// <param name="numBatchItems"></param>
	private void EnsureArrayCapacity(int numBatchItems)
	{
		int neededCapacity = 6 * numBatchItems;
		if (_index == null || neededCapacity > _index.Length)
		{
			short[] newIndex = new short[6 * numBatchItems];
			int start = 0;
			if (_index != null)
			{
				_index.CopyTo(newIndex, 0);
				start = _index.Length / 6;
			}
			for (int i = start; i < numBatchItems; i++)
			{
				newIndex[i * 6] = (short)(i * 4);
				newIndex[i * 6 + 1] = (short)(i * 4 + 1);
				newIndex[i * 6 + 2] = (short)(i * 4 + 2);
				newIndex[i * 6 + 3] = (short)(i * 4 + 1);
				newIndex[i * 6 + 4] = (short)(i * 4 + 3);
				newIndex[i * 6 + 5] = (short)(i * 4 + 2);
			}
			_index = newIndex;
			_vertexArray = new VertexPositionColorTexture[4 * numBatchItems];
		}
	}

	private void EnsureSimpleArrayCapacity(int numBatchItems)
	{
		int neededCapacity = 6 * numBatchItems;
		if (_simpleIndex == null || neededCapacity > _simpleIndex.Length)
		{
			short[] newIndex = new short[6 * numBatchItems];
			int start = 0;
			if (_simpleIndex != null)
			{
				_simpleIndex.CopyTo(newIndex, 0);
				start = _simpleIndex.Length / 6;
			}
			for (int i = start; i < numBatchItems; i++)
			{
				newIndex[i * 6] = (short)(i * 4);
				newIndex[i * 6 + 1] = (short)(i * 4 + 1);
				newIndex[i * 6 + 2] = (short)(i * 4 + 2);
				newIndex[i * 6 + 3] = (short)(i * 4 + 1);
				newIndex[i * 6 + 4] = (short)(i * 4 + 3);
				newIndex[i * 6 + 5] = (short)(i * 4 + 2);
			}
			_simpleIndex = newIndex;
			_simpleVertexArray = new VertexPositionColor[4 * numBatchItems];
		}
	}

	private void EnsureGeometryArrayCapacity(int numTris)
	{
		int neededCapacity = 3 * numTris;
		if (_geometryIndex == null || neededCapacity > _geometryIndex.Length)
		{
			short[] newIndex = new short[3 * numTris];
			int start = 0;
			if (_geometryIndex != null)
			{
				_geometryIndex.CopyTo(newIndex, 0);
				start = _geometryIndex.Length / 3;
			}
			for (int i = start; i < numTris; i++)
			{
				newIndex[i * 3] = (short)(i * 3);
				newIndex[i * 3 + 1] = (short)(i * 3 + 1);
				newIndex[i * 3 + 2] = (short)(i * 3 + 2);
			}
			_geometryIndex = newIndex;
			_geometryVertexArray = new VertexPositionColor[4 * numTris];
		}
	}

	private void EnsureTexturedGeometryArrayCapacity(int numTris)
	{
		int neededCapacity = 3 * numTris;
		if (_texturedGeometryIndex == null || neededCapacity > _texturedGeometryIndex.Length)
		{
			short[] newIndex = new short[3 * numTris];
			int start = 0;
			if (_texturedGeometryIndex != null)
			{
				_texturedGeometryIndex.CopyTo(newIndex, 0);
				start = _texturedGeometryIndex.Length / 3;
			}
			for (int i = start; i < numTris; i++)
			{
				newIndex[i * 3] = (short)(i * 3);
				newIndex[i * 3 + 1] = (short)(i * 3 + 1);
				newIndex[i * 3 + 2] = (short)(i * 3 + 2);
			}
			_texturedGeometryIndex = newIndex;
			_geometryVertexArrayTextured = new VertexPositionColorTexture[4 * numTris];
		}
	}

	/// <summary>
	/// Reference comparison of the underlying Texture objects for each given MTSpriteBatchitem.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns>0 if they are not reference equal, and 1 if so.</returns>
	private static int CompareTextureFunc(MTSpriteBatchItem a, MTSpriteBatchItem b)
	{
		if (a.Texture != b.Texture)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// Compares the Depth of a against b returning -1 if a is less than b, 
	/// 0 if equal, and 1 if a is greater than b. The test uses float.CompareTo(float)
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns>-1 if a is less than b, 0 if equal, and 1 if a is greater than b</returns>
	private static int CompareDepthFunc(MTSpriteBatchItem a, MTSpriteBatchItem b)
	{
		return a.Depth.CompareTo(b.Depth);
	}

	private static int CompareSimpleDepthFunc(MTSimpleSpriteBatchItem a, MTSimpleSpriteBatchItem b)
	{
		return 0;
	}

	private static int CompareGeometryDepthFunc(GeometryItem a, GeometryItem b)
	{
		return a.depth.CompareTo(b.depth);
	}

	private static int CompareTexturedGeometryDepthFunc(GeometryItemTexture a, GeometryItemTexture b)
	{
		return a.depth.CompareTo(b.depth);
	}

	/// <summary>
	/// Implements the opposite of CompareDepth, where b is compared against a.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns>-1 if b is less than a, 0 if equal, and 1 if b is greater than a</returns>
	private static int CompareReverseDepthFunc(MTSpriteBatchItem a, MTSpriteBatchItem b)
	{
		return b.Depth.CompareTo(a.Depth);
	}

	private static int CompareSimpleReverseDepthFunc(MTSimpleSpriteBatchItem a, MTSimpleSpriteBatchItem b)
	{
		return 0;
	}

	private static int CompareGeometryReverseDepthFunc(GeometryItem a, GeometryItem b)
	{
		return b.depth.CompareTo(a.depth);
	}

	private static int CompareTexturedGeometryReverseDepthFunc(GeometryItemTexture a, GeometryItemTexture b)
	{
		return b.depth.CompareTo(a.depth);
	}

	/// <summary>
	/// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
	/// overflow the 16 bit array indices for vertices.
	/// </summary>
	/// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
	public void DrawBatch(SpriteSortMode sortMode)
	{
		if (_batchItemList.Count == 0)
		{
			return;
		}
		switch (sortMode)
		{
		case SpriteSortMode.Texture:
			DGList.Sort(_batchItemList, CompareTexture);
			break;
		case SpriteSortMode.FrontToBack:
			DGList.Sort(_batchItemList, CompareDepth);
			break;
		case SpriteSortMode.BackToFront:
			DGList.Sort(_batchItemList, CompareReverseDepth);
			break;
		}
		int batchIndex = 0;
		int batchCount = _batchItemList.Count;
		while (batchCount > 0)
		{
			int startIndex = 0;
			int index = 0;
			Texture2D tex = null;
			Material eff = null;
			int numBatchesToProcess = batchCount;
			if (numBatchesToProcess > 5461)
			{
				numBatchesToProcess = 5461;
			}
			EnsureArrayCapacity(numBatchesToProcess);
			int i = 0;
			while (i < numBatchesToProcess)
			{
				MTSpriteBatchItem item = _batchItemList[batchIndex];
				if (item.Texture != tex || item.Material != eff)
				{
					FlushVertexArray(startIndex, index);
					//if (eff != null && item.Material == null)
					//{
					//	_batch.Setup();
					//}
					eff = (_batch.transitionEffect ? null : item.Material);
					tex = item.Texture;
					startIndex = (index = 0);
					_device.Textures[0] = tex;
					if (eff != null)
					{
						eff.SetValue("MatrixTransform", _batch.fullMatrix);
						eff.Apply();
					}
					else //new
					{
						_batch.Setup();
					}
                }
				_vertexArray[index++] = item.vertexTL;
				_vertexArray[index++] = item.vertexTR;
				_vertexArray[index++] = item.vertexBL;
				_vertexArray[index++] = item.vertexBR;
				if (item.inPool)
				{
					item.Texture = null;
					item.Material = null;
					_freeBatchItemQueue.Enqueue(item);
				}
				i++;
				batchIndex++;
			}
			FlushVertexArray(startIndex, index);
			batchCount -= numBatchesToProcess;
		}
		_batchItemList.Clear();
	}

	public void DrawSimpleBatch(SpriteSortMode sortMode)
	{
		if (_simpleBatchItemList.Count == 0)
		{
			return;
		}
		switch (sortMode)
		{
		case SpriteSortMode.FrontToBack:
			DGList.Sort(_simpleBatchItemList, CompareSimpleDepth);
			break;
		case SpriteSortMode.BackToFront:
			DGList.Sort(_simpleBatchItemList, CompareSimpleReverseDepth);
			break;
		}
		int batchIndex = 0;
		int batchCount = _simpleBatchItemList.Count;
		while (batchCount > 0)
		{
			int startIndex = 0;
			int index = 0;
			int numBatchesToProcess = batchCount;
			if (numBatchesToProcess > 5461)
			{
				numBatchesToProcess = 5461;
			}
			EnsureSimpleArrayCapacity(numBatchesToProcess);
			int i = 0;
			while (i < numBatchesToProcess)
			{
				MTSimpleSpriteBatchItem item = _simpleBatchItemList[batchIndex];
				_simpleVertexArray[index++] = item.vertexTL;
				_simpleVertexArray[index++] = item.vertexTR;
				_simpleVertexArray[index++] = item.vertexBL;
				_simpleVertexArray[index++] = item.vertexBR;
				_freeSimpleBatchItemQueue.Enqueue(item);
				i++;
				batchIndex++;
			}
			FlushSimpleVertexArray(startIndex, index);
			batchCount -= numBatchesToProcess;
		}
		_simpleBatchItemList.Clear();
	}

	public void DrawGeometryBatch(SpriteSortMode sortMode)
	{
		if (_geometryBatch.Count == 0)
		{
			return;
		}
		switch (sortMode)
		{
		case SpriteSortMode.FrontToBack:
			DGList.Sort(_geometryBatch, CompareGeometryDepth);
			break;
		case SpriteSortMode.BackToFront:
			DGList.Sort(_geometryBatch, CompareGeometryReverseDepth);
			break;
		}
		int size = 0;
		foreach (GeometryItem item in _geometryBatch)
		{
			size += item.length;
		}
		EnsureGeometryArrayCapacity((size + 1) / 3);
		Material eff = null;
		int startIndex = 0;
		int index = 0;
		foreach (GeometryItem item2 in _geometryBatch)
		{
			if (item2.material != eff)
			{
				FlushGeometryVertexArray(startIndex, index);
				if (eff != null && item2.material == null)
				{
					_batch.ReapplyEffect(simple: true);
				}
				eff = item2.material;
				eff?.Apply();
			}
			for (int i = 0; i < item2.length; i += 3)
			{
				_geometryVertexArray[index++] = item2.vertices[i];
				_geometryVertexArray[index++] = item2.vertices[i + 1];
				_geometryVertexArray[index++] = item2.vertices[i + 2];
			}
			if (item2.temporary)
			{
				_freeGeometryBatch.Enqueue(item2);
			}
		}
		FlushGeometryVertexArray(startIndex, index);
		_geometryBatch.Clear();
	}

	public void DrawTexturedGeometryBatch(SpriteSortMode sortMode)
	{
		if (_geometryBatchTextured.Count == 0)
		{
			return;
		}
		switch (sortMode)
		{
		case SpriteSortMode.FrontToBack:
			DGList.Sort(_geometryBatchTextured, CompareTexturedGeometryDepth);
			break;
		case SpriteSortMode.BackToFront:
			DGList.Sort(_geometryBatchTextured, CompareTexturedGeometryReverseDepth);
			break;
		}
		int size = 0;
		foreach (GeometryItemTexture item in _geometryBatchTextured)
		{
			size += item.length;
		}
		EnsureTexturedGeometryArrayCapacity((size + 1) / 3);
		Texture2D tex = null;
		int startIndex = 0;
		int index = 0;
		foreach (GeometryItemTexture item2 in _geometryBatchTextured)
		{
			if (item2.texture != tex)
			{
				FlushTexturedGeometryVertexArray(startIndex, index);
				tex = item2.texture;
				startIndex = (index = 0);
				_device.Textures[0] = tex;
			}
			for (int i = 0; i < item2.length; i += 3)
			{
				_geometryVertexArrayTextured[index++] = item2.vertices[i];
				_geometryVertexArrayTextured[index++] = item2.vertices[i + 1];
				_geometryVertexArrayTextured[index++] = item2.vertices[i + 2];
			}
			if (item2.temporary)
			{
				_freeGeometryBatchTextured.Enqueue(item2);
			}
			item2.texture = null;
			FlushTexturedGeometryVertexArray(startIndex, index);
		}
		_geometryBatchTextured.Clear();
	}

	/// <summary>
	/// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
	/// </summary>
	/// <param name="start">Start index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
	/// <param name="end">End index of vertices to draw. Not used except to compute the count of vertices to draw.</param>
	private void FlushVertexArray(int start, int end)
	{
		if (start != end)
		{
			int vertexCount = end - start;
			_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexArray, 0, vertexCount, _index, 0, vertexCount / 4 * 2, VertexPositionColorTexture.VertexDeclaration);
		}
	}

	private void FlushSimpleVertexArray(int start, int end)
	{
		if (start != end)
		{
			int vertexCount = end - start;
			_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _simpleVertexArray, 0, vertexCount, _simpleIndex, 0, vertexCount / 4 * 2, VertexPositionColor.VertexDeclaration);
		}
	}

	private void FlushGeometryVertexArray(int start, int end)
	{
		if (start != end)
		{
			int vertexCount = end - start;
			_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _geometryVertexArray, 0, vertexCount, _geometryIndex, 0, vertexCount / 3, VertexPositionColor.VertexDeclaration);
		}
	}

	private void FlushTexturedGeometryVertexArray(int start, int end)
	{
		if (start != end)
		{
			int vertexCount = end - start;
			_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _geometryVertexArrayTextured, 0, vertexCount, _texturedGeometryIndex, 0, vertexCount / 3, VertexPositionColorTexture.VertexDeclaration);
		}
	}
}
