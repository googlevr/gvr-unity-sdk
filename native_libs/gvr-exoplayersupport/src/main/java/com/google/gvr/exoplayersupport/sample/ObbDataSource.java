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

import android.content.res.AssetFileDescriptor;
import android.util.Log;

import com.google.android.exoplayer.C;
import com.google.android.exoplayer.upstream.DataSource;
import com.google.android.exoplayer.upstream.DataSpec;
import com.google.android.exoplayer.upstream.TransferListener;

import java.io.BufferedInputStream;
import java.io.EOFException;
import java.io.FileNotFoundException;
import java.io.IOException;

/** Data source for reading assets from an OBB or jar file. */
public class ObbDataSource implements DataSource {

  private static final String TAG = "ObbDataSource";

  private TransferListener transferListener;

  private ZipResourceFile zip;
  private AssetFileDescriptor fd;
  private BufferedInputStream inStream;
  private long bytesRemaining;

  /**
   * Constructs a new instance.
   *
   * <p>
   *
   * @param listener An optional {@link TransferListener}.
   */
  public ObbDataSource(TransferListener listener) {
    this.transferListener = listener;
  }

  /**
   * Opens the {@link DataSource} to read the specified data. Calls to {@link #open(DataSpec)} and
   * {@link #close()} must be balanced.
   *
   * <p>Note: If {@link #open(DataSpec)} throws an {@link IOException}, callers must still call
   * {@link #close()} to ensure that any partial effects of the {@link #open(DataSpec)} invocation
   * are cleaned up. Implementations of this class can assume that callers will call {@link
   * #close()} in this case.
   *
   * @param dataSpec Defines the data to be read.
   * @return The number of bytes that can be read from the opened source. For unbounded requests
   *     (i.e. requests where {@link DataSpec#length} equals LENGTH_UNBOUNDED) this value is the
   *     resolved length of the request, or LENGTH_UNBOUNDED if the length is still unresolved. For
   *     all other requests, the value returned will be equal to the request's {@link
   *     DataSpec#length}.
   * @throws IOException If an error occurs opening the source.
   */
  @Override
  public long open(DataSpec dataSpec) throws IOException {

    Log.d(TAG, "Open called: " + dataSpec.uri);
    if (dataSpec.uri.toString().startsWith("jar:file://")) {

      String zipFile = dataSpec.uri.toString().substring("jar:file://".length());
      String assetFile = null;
      int idx = zipFile.indexOf("!");
      if (idx > 0) {
        assetFile = zipFile.substring(idx + 1);
        zipFile = zipFile.substring(0, idx);
        if (assetFile.startsWith("/")) {
          assetFile = assetFile.substring(1);
        }
      }
      Log.d(TAG, " Reading [" + assetFile + "] from " + zipFile);
      zip = new ZipResourceFile(zipFile);
      fd = zip.getAssetFileDescriptor(assetFile);

      if (fd != null) {
        inStream = new BufferedInputStream(fd.createInputStream());
        if (dataSpec.position > 0) {
          inStream.skip(dataSpec.position);
        }
        if (dataSpec.length == C.LENGTH_UNBOUNDED) {
          bytesRemaining = fd.getDeclaredLength() - dataSpec.position;
        } else {
          bytesRemaining = dataSpec.length;
        }
        if (bytesRemaining < 0) {
          throw new EOFException();
        }
        Log.d(TAG, "Returning length : " + bytesRemaining);
        return bytesRemaining;
      } else {
        Log.w(TAG, "Could not get fd for " + assetFile);
        Log.d(TAG, " There are " + zip.getAllEntries().length + " entries:");
        for (ZipResourceFile.ZipEntryRO ent : zip.getAllEntries()) {
          Log.d(TAG, "Entry: " + ent.mFileName);
        }
      }
      throw new FileNotFoundException("Could not get " + dataSpec.uri);
    }
    throw new IOException("Data Uri does not start with 'jar:file://'");
  }

  /**
   * Closes the {@link DataSource}.
   *
   * <p>Note: This method will be called even if the corresponding call to {@link #open(DataSpec)}
   * threw an {@link IOException}. See {@link #open(DataSpec)} for more details.
   *
   * @throws IOException If an error occurs closing the source.
   */
  @Override
  public void close() throws IOException {

    Log.d(TAG, "Closing");
    if (inStream != null) {
      try {
        inStream.close();
      } finally {
        inStream = null;
      }
    }
    if (fd != null) {
      try {
        fd.close();
      } finally {
        fd = null;
      }
    }
    zip = null;
  }

  /**
   * Reads up to {@code length} bytes of data and stores them into {@code buffer}, starting at index
   * {@code offset}.
   *
   * <p>This method blocks until at least one byte of data can be read, the end of the opened range
   * is detected, or an exception is thrown.
   *
   * @param buffer The buffer into which the read data should be stored.
   * @param offset The start offset into {@code buffer} at which data should be written.
   * @param readLength The maximum number of bytes to read.
   * @return The number of bytes read, or RESULT_END_OF_INPUT if the end of the opened range is
   *     reached.
   * @throws IOException If an error occurs reading from the source.
   */
  @Override
  public int read(byte[] buffer, int offset, int readLength) throws IOException {
    if (bytesRemaining == 0) {
      return -1;
    } else {
      int bytesRead = inStream.read(buffer, offset, (int) Math.min(bytesRemaining, readLength));
      if (bytesRead > 0) {
        bytesRemaining -= bytesRead;
        if (transferListener != null) {
          transferListener.onBytesTransferred(bytesRead);
        }
      }
      return bytesRead;
    }
  }
}
