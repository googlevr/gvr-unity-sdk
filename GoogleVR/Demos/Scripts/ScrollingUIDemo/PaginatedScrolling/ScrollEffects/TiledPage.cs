// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class TiledPage : MonoBehaviour {
  /// Allows you to assign a custom set of tiles
  /// To animate when this page is scrolling.
  [SerializeField]
  [Tooltip("The tiles to animate when scrolling.")]
  private Transform[] tiles;

  /// The RectTransform that tiles animate relative to.
  /// The width and height of the layout transform will control
  /// when the tiles start animating while scrolling.
  [SerializeField]
  [Tooltip("The RectTransform that tiles animate relative to.")]
  private RectTransform layoutTransform;

  /// Controls how much the tiles move when they are animating.
  /// Set to 0 to turn off animation.
  [SerializeField]
  [Tooltip("Controls how much the tiles move when they are animating.")]
  private float staggerAnimationIntensity = 0.5f;

  public enum TileOrderBy {
    Center,
    LeftEdge,
    LeftEdgeBySize,
    RightEdge,
    RightEdgeBySize
  }

  /// Controls the order that tiles move in when they are animating.
  /// This is useful when a page has non-uniform tiles.
  [SerializeField]
  [Tooltip("Controls the order that tiles move in when they are animating.")]
  private TileOrderBy tileOrderBy = TileOrderBy.Center;

  /// The Key is an x position relative to the left side of the layoutTransform.
  /// The value is a list of tiles that exist at that x position.
  private SortedDictionary<float, List<Transform>> tilesByDistanceFromLeft;

  /// When the distance between two tiles is within
  /// TileGroupThreshold from eachother, they
  /// Are considered within the same tile group
  /// For animation purposes.
  private const float kTileGroupThreshold = 5.0f;

  /// <summary>
  /// Call if the layout of tiles on this page has changed.
  /// This will flush the cache to make sure the staggered
  /// tiles animation plays correctly.
  /// </summary>
  public void FlushLayoutCache() {
    tilesByDistanceFromLeft = null;
  }

  /// <summary>
  /// Called by PagedScrollRect when scrolling occurs.
  /// Do not call manually.
  /// </summary>
  /// <param name="scrollDistance">Signed scroll distance for this page.</param>
  /// <param name="scrollSpacing">Spacing between pages.</param>
  /// <param name="isInteractable">True is the PagedScrollRect is currently scrolling.</param>
  public void ApplyScrollEffect(float scrollDistance, float scrollSpacing, bool isInteractable) {
    /// Organize the tiles by their x position
    /// So that we can stagger them correctly.
    CalculateTilesByDistance();

    IEnumerable<KeyValuePair<float, List<Transform>>> iterator;
    float directionCoeff;

    if (scrollDistance > 0) {
      /// Scrolling Left
      iterator = tilesByDistanceFromLeft;
      directionCoeff = -1.0f;
    } else {
      /// Scrolling Right
      iterator = tilesByDistanceFromLeft.Reverse();
      directionCoeff = 1.0f;
    }

    float scrollMagnitude = Mathf.Abs(scrollDistance);
    float ratioScrolled = scrollMagnitude / scrollSpacing;
    int index = 0;

    bool updatedAnimatingTiles = false;

    foreach (var pair in iterator) {
      float tileGroupRatio = (index + 1.0f) / tilesByDistanceFromLeft.Count;
      float tileGroupInterval = scrollSpacing / tilesByDistanceFromLeft.Count;
      tileGroupInterval *= staggerAnimationIntensity;

      /// These tiles are currently animating based on the
      /// Amount that the user has scrolled the scroll rect.
      if (ratioScrolled < tileGroupRatio && !updatedAnimatingTiles) {
        foreach (Transform tile in pair.Value) {
          float offset = tileGroupInterval * index;
          float animatedXPos = (scrollMagnitude * staggerAnimationIntensity * directionCoeff) - (offset * directionCoeff);

          RectTransform cellRect = GetTileCell(tile);
          Vector3 position = tile.position;
          position.x = cellRect.TransformPoint(new Vector3(animatedXPos, 0.0f, 0.0f)).x;
          UpdateTile(tile, position, isInteractable);
        }
        updatedAnimatingTiles = true;
      } else {
        /// These tiles have not been animated yet,
        /// Make sure their local position is reset.
        if (updatedAnimatingTiles) {
          foreach (Transform tile in pair.Value) {
            RectTransform cellRect = GetTileCell(tile);
            Vector3 position = tile.position;
            position.x = cellRect.TransformPoint(Vector3.zero).x;
            UpdateTile(tile, position, isInteractable);
          }
        } else {
          /// These tiles have already finished animating
          /// Make sure they snap to their final position.
          foreach (Transform tile in pair.Value) {
            RectTransform cellRect = GetTileCell(tile);
            Vector3 position = tile.position;
            position.x = cellRect.TransformPoint(new Vector3(tileGroupInterval * directionCoeff, 0.0f, 0.0f)).x;
            UpdateTile(tile, position, isInteractable);
          }
        }
      }

      index += 1;
    }
  }

  private void UpdateTile(Transform tile, Vector3 position, bool isInteractable) {
    tile.position = position;

    Tile Tile = tile.GetComponent<Tile>();
    if (Tile != null) {
      Tile.IsInteractable = isInteractable;
    }
  }

  private void CalculateTilesByDistance() {
    /// Only do this if we haven't already calculated it.
    if (tilesByDistanceFromLeft != null) {
      return;
    }

    Canvas.ForceUpdateCanvases();

    tilesByDistanceFromLeft = new SortedDictionary<float, List<Transform>>();

    foreach (Transform tile in tiles) {
      RectTransform cellRect = GetTileCell(tile);
      RectTransform tileRect = tile.GetComponent<RectTransform>();

      /// Find how far this cell is from the left side of the layout.
      Vector3 tilePoint = GetTilePoint(tileRect);
      Vector3 worldPoint = cellRect.TransformPoint(tilePoint);
      Vector3 layoutPoint = layoutTransform.InverseTransformPoint(worldPoint);
      float distanceFromLeft = layoutPoint.x - layoutTransform.rect.xMin;

      /// Add the tile into the appropriate group based on it's x position.
      List<Transform> tilesAtDistance;
      if (tilesByDistanceFromLeft.TryGetValue(distanceFromLeft, out tilesAtDistance)) {
        tilesAtDistance.Add(tile);
      } else {
        /// See if their is already a tile group that exists
        /// Within range of the TileGroupThreshold.
        tilesAtDistance = tilesByDistanceFromLeft.FirstOrDefault(
          pair => {
            float distance = Mathf.Abs(distanceFromLeft - pair.Key);
            return distance < kTileGroupThreshold;
          }).Value;

        /// Found a tile group within range.
        if (tilesAtDistance != null) {
          tilesAtDistance.Add(tile);
        } else {
          tilesAtDistance = new List<Transform>();
          tilesAtDistance.Add(tile);
          tilesByDistanceFromLeft[distanceFromLeft] = tilesAtDistance;
        }
      }
    }
  }

  private Vector3 GetTilePoint(RectTransform tileRect) {
    switch (tileOrderBy) {
      case TileOrderBy.Center:
        return tileRect.rect.center;
      case TileOrderBy.LeftEdge:
        return tileRect.rect.min;
      case TileOrderBy.LeftEdgeBySize:
        return tileRect.rect.min - (tileRect.rect.size * 0.5f);
      case TileOrderBy.RightEdge:
        return tileRect.rect.max;
      case TileOrderBy.RightEdgeBySize:
        return tileRect.rect.max + (tileRect.rect.size * 0.5f);
      default:
        return Vector3.zero;
    }
  }

  private RectTransform GetTileCell(Transform tile) {
    RectTransform cellRect;

    Tile Tile = tile.GetComponent<Tile>();
    if (Tile != null) {
      cellRect = Tile.Cell;
    } else {
      cellRect = tile.parent.GetComponent<RectTransform>();
    }

    return cellRect;
  }
}
