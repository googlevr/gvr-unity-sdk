/*
 * Copyright (C) 2016 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.google.gvr.exoplayersupport.sample;

import com.google.android.exoplayer.extractor.ts.PtsTimestampAdjuster;
import com.google.android.exoplayer.hls.HlsChunkSource;
import com.google.android.exoplayer.hls.HlsMasterPlaylist;
import com.google.android.exoplayer.hls.HlsPlaylist;
import com.google.android.exoplayer.hls.HlsTrackSelector;
import com.google.android.exoplayer.hls.PtsTimestampAdjusterProvider;
import com.google.android.exoplayer.hls.Variant;
import com.google.android.exoplayer.upstream.BandwidthMeter;
import com.google.android.exoplayer.upstream.DataSource;

/**
 * Extended Exoplayer HlsChunkSource. This accomodates selecting an initial resolution to attempt
 * playback with.
 */
public class GVRHlsChunkSource extends HlsChunkSource {

  private int targetResolution;

  /**
   * @param isMaster True if this is the master source for the playback. False otherwise. Each
   *     playback must have exactly one master source, which should be the source providing video
   *     chunks (or audio chunks for audio only playbacks).
   * @param dataSource A {@link DataSource} suitable for loading the media data.
   * @param playlist The HLS playlist.
   * @param trackSelector Selects tracks to be exposed by this source.
   * @param bandwidthMeter Provides an estimate of the currently available bandwidth.
   * @param timestampAdjusterProvider A provider of {@link PtsTimestampAdjuster} instances.
   * @param targetResolution - the target height for the initial video stream. The stream selected
   *     will be the highest resolution available with a height less than or equal to the
   *     targetResolution value.
   */
  public GVRHlsChunkSource(
      boolean isMaster,
      DataSource dataSource,
      HlsPlaylist playlist,
      HlsTrackSelector trackSelector,
      BandwidthMeter bandwidthMeter,
      PtsTimestampAdjusterProvider timestampAdjusterProvider,
      int targetResolution) {
    super(isMaster, dataSource, playlist, trackSelector, bandwidthMeter, timestampAdjusterProvider);
    this.targetResolution = targetResolution;
  }

  @Override
  protected int computeDefaultVariantIndex(
      HlsMasterPlaylist playlist, Variant[] variants, BandwidthMeter bandwidthMeter) {

    int defaultIndex = -1;
    int maxBitrate = 0;
    int minBitrate = Integer.MAX_VALUE;
    int lowestResolutionIndex = -1;

    if (bandwidthMeter.getBitrateEstimate() == BandwidthMeter.NO_ESTIMATE) {

      if (targetResolution > 0) {
        for (int i = 0; i < variants.length; i++) {
          Variant variant = variants[i];
          if (variant.format.height <= targetResolution && variant.format.bitrate > maxBitrate) {
            maxBitrate = variant.format.bitrate;
            defaultIndex = i;
          }
          if (variant.format.bitrate < minBitrate) {
            minBitrate = variant.format.bitrate;
            lowestResolutionIndex = i;
          }
        }
      }
      if (defaultIndex >= 0) {
        return defaultIndex;
      }

      if (lowestResolutionIndex >= 0) {
        return lowestResolutionIndex;
      }
    }
    return super.computeDefaultVariantIndex(playlist, variants, bandwidthMeter);
  }
}
