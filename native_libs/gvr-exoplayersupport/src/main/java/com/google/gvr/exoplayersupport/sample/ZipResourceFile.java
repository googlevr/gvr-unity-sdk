/*
 * Copyright (C) 2012 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.google.gvr.exoplayersupport.sample;

import android.content.res.AssetFileDescriptor;
import android.os.ParcelFileDescriptor;
import android.util.Log;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.RandomAccessFile;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.MappedByteBuffer;
import java.nio.channels.FileChannel;
import java.util.Collection;
import java.util.HashMap;
import java.util.Vector;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

/** Read-only access to Zip archives, with minimal heap allocation. */
public class ZipResourceFile {
  private static final String LOG_TAG = "zipro";
  private static final boolean LOGV = false;

  // 4-byte number.

  /*
   * Zip file constants.
   */
  private static final int EOCD_SIGNATURE = 0x06054b50;
  private static final int EOCD_LEN = 22;
  private static final int EOCD_NUM_ENTRIES = 8; // offset to #of entries in file
  private static final int EOCD_SIZE = 12; // size of the central directory
  private static final int EOCD_FILE_OFFSET = 16; // offset to central directory
  private static final int MAX_COMMENT_LEN = 65535; // longest possible in ushort
  private static final int MAX_EOCD_SEARCH = (MAX_COMMENT_LEN + EOCD_LEN);
  private static final int LFH_SIGNATURE = 0x04034b50;
  private static final int LFH_LEN = 30; // excluding variable-len fields
  private static final int LFH_NAME_LEN = 26; // offset to filename length
  private static final int LFH_EXTRA_LEN = 28; // offset to extra length
  private static final int CDE_SIGNATURE = 0x02014b50;
  private static final int CDE_LEN = 46; // excluding variable-len fields
  private static final int CDE_METHOD = 10; // offset to compression method
  private static final int CDE_MOD_WHEN = 12; // offset to modification timestamp
  private static final int CDECRC = 16; // offset to entry CRC
  private static final int CDE_COMP_LEN = 20; // offset to compressed length
  private static final int CDE_UNCOMP_LEN = 24; // offset to uncompressed length
  private static final int CDE_NAME_LEN = 28; // offset to filename length
  private static final int CDE_EXTRA_LEN = 30; // offset to extra length
  private static final int CDE_COMMENT_LEN = 32; // offset to comment length
  private static final int CDE_LOCAL_OFFSET = 42; // offset to local hdr
  private static final int COMPRESS_STORED = 0; // no compression
  private static final int COMPRESS_DEFLATED = 8; // standard deflate
  /*
   * The values we return for ZipEntryRO use 0 as an invalid value, so we want
   * to adjust the hash table index by a fixed amount. Using a large value
   * helps insure that people don't mix & match arguments, e.g. to
   * findEntryByIndex().
   */
  private static final int ZIP_ENTRY_ADJ = 10000;

  /** ZipEntry used when iterating over the zip file contents. */
  public static final class ZipEntryRO {
    public ZipEntryRO(final String zipFileName, final File file, final String fileName) {
      mFileName = fileName;
      mZipFileName = zipFileName;
      mFile = file;
    }

    public final File mFile;
    public final String mFileName;
    public final String mZipFileName;
    public long mLocalHdrOffset; // offset of local file header
    /* useful stuff from the directory entry */
    public int mMethod;
    public long mWhenModified;
    public long mCRC32;
    public long mCompressedLength;
    public long mUncompressedLength;
    public long mOffset = -1;
    
    public void setOffsetFromFile(RandomAccessFile f, ByteBuffer buf) throws IOException {
      long localHdrOffset = mLocalHdrOffset;
      try {
        f.seek(localHdrOffset);
        f.readFully(buf.array());
        if (buf.getInt(0) != LFH_SIGNATURE) {
          Log.w(LOG_TAG, "didn't find signature at start of lfh");
          throw new IOException();
        }
        int nameLen = buf.getShort(LFH_NAME_LEN) & 0xFFFF;
        int extraLen = buf.getShort(LFH_EXTRA_LEN) & 0xFFFF;
        mOffset = localHdrOffset + LFH_LEN + nameLen + extraLen;
      } catch (IOException ioe) {
        Log.e(LOG_TAG, "Cannot setOffset", ioe);
      }
    }

    /**
     * Calculates the offset of the start of the Zip file entry within the Zip file.
     *
     * @return the offset, in bytes from the start of the file of the entry
     */
    public long getOffset() {
      return mOffset;
    }

    /**
     * isUncompressed
     *
     * @return true if the file is stored in uncompressed form
     */
    public boolean isUncompressed() {
      return mMethod == COMPRESS_STORED;
    }

    public AssetFileDescriptor getAssetFileDescriptor() throws IOException {
      if (mMethod == COMPRESS_STORED) {
        ParcelFileDescriptor pfd;
        pfd = ParcelFileDescriptor.open(mFile, ParcelFileDescriptor.MODE_READ_ONLY);
        return new AssetFileDescriptor(pfd, getOffset(), mUncompressedLength);
      } else {
        throw new IOException("File is not stored uncompressed!");
      }
    }

    public String getZipFileName() {
      return mZipFileName;
    }

    public File getZipFile() {
      return mFile;
    }
  }

  private HashMap<String, ZipEntryRO> mHashMap = new HashMap<String, ZipEntryRO>();
  /* for reading compressed files */
  public HashMap<File, ZipFile> mZipFiles = new HashMap<File, ZipFile>();

  public ZipResourceFile(String zipFileName) throws IOException {
    addPatchFile(zipFileName);
  }

  ZipEntryRO[] getEntriesAt(String path) {
    Vector<ZipEntryRO> zev = new Vector<ZipEntryRO>();
    Collection<ZipEntryRO> values = mHashMap.values();
    if (null == path) {
      path = "";
    }
    int length = path.length();
    for (ZipEntryRO ze : values) {
      if (ze.mFileName.startsWith(path)) {
        if (-1 == ze.mFileName.indexOf('/', length)) {
          zev.add(ze);
        }
      }
    }
    ZipEntryRO[] entries = new ZipEntryRO[zev.size()];
    return zev.toArray(entries);
  }

  public ZipEntryRO[] getAllEntries() {
    Collection<ZipEntryRO> values = mHashMap.values();
    return values.toArray(new ZipEntryRO[values.size()]);
  }

  /**
   * getAssetFileDescriptor allows for ZipResourceFile to directly feed Android API's that want an
   * fd, offset, and length such as the MediaPlayer. It also allows for the class to be used in a
   * content provider that can feed video players. The file must be stored (non-compressed) in the
   * Zip file for this to work.
   *
   * @param assetPath
   * @return the asset file descriptor for the file, or null if the file isn't present or is stored
   *     compressed
   */
  public AssetFileDescriptor getAssetFileDescriptor(String assetPath) throws IOException {
    ZipEntryRO entry = mHashMap.get(assetPath);
    if (null != entry) {
      return entry.getAssetFileDescriptor();
    }
    return null;
  }

  /**
   * getInputStream returns an AssetFileDescriptor.AutoCloseInputStream associated with the asset
   * that is contained in the Zip file, or a standard ZipInputStream if necessary to uncompress the
   * file
   *
   * @param assetPath
   * @return an input stream for the named asset path, or null if not found
   * @throws IOException
   */
  public InputStream getInputStream(String assetPath) throws IOException {
    ZipEntryRO entry = mHashMap.get(assetPath);
    if (null != entry) {
      if (entry.isUncompressed()) {
        return entry.getAssetFileDescriptor().createInputStream();
      } else {
        ZipFile zf = mZipFiles.get(entry.getZipFile());
        /** read compressed files * */
        if (null == zf) {
          zf = new ZipFile(entry.getZipFile(), ZipFile.OPEN_READ);
          mZipFiles.put(entry.getZipFile(), zf);
        }
        ZipEntry zi = zf.getEntry(assetPath);
        if (null != zi) {
          return zf.getInputStream(zi);
        }
      }
    }
    return null;
  }

  private static int read4LE(RandomAccessFile f) throws IOException {
    return swapEndian(f.readInt());
  }

  /*
   * Opens the specified file read-only. We memory-map the entire thing and
   * close the file before returning.
   */
  void addPatchFile(String zipFileName) throws IOException {
    File file = new File(zipFileName);
    RandomAccessFile f = new RandomAccessFile(file, "r");
    long fileLength = f.length();
    if (fileLength < EOCD_LEN) {
      throw new java.io.IOException("fileLength < EOCDlen");
    }
    long readAmount = MAX_EOCD_SEARCH;
    if (readAmount > fileLength) {
      readAmount = fileLength;
    }
    /*
     * Make sure this is a Zip archive.
     */
    f.seek(0);
    int header = read4LE(f);
    if (header == EOCD_SIGNATURE) {
      Log.i(LOG_TAG, "Found Zip archive, but it looks empty");
      throw new IOException();
    } else if (header != LFH_SIGNATURE) {
      Log.v(LOG_TAG, "Not a Zip archive");
      throw new IOException();
    }
    /*
     * Perform the traditional EOCD snipe hunt. We're searching for the End
     * of Central Directory magic number, which appears at the start of the
     * EOCD block. It's followed by 18 bytes of EOCD stuff and up to 64KB of
     * archive comment. We need to read the last part of the file into a
     * buffer, dig through it to find the magic number, parse some values
     * out, and use those to determine the extent of the CD. We start by
     * pulling in the last part of the file.
     */
    long searchStart = fileLength - readAmount;
    f.seek(searchStart);
    ByteBuffer bbuf = ByteBuffer.allocate((int) readAmount);
    byte[] buffer = bbuf.array();
    f.readFully(buffer);
    bbuf.order(ByteOrder.LITTLE_ENDIAN);
    /*
     * Scan backward for the EOCD magic. In an archive without a trailing
     * comment, we'll find it on the first try. (We may want to consider
     * doing an initial minimal read; if we don't find it, retry with a
     * second read as above.)
     */
    // EOCD == 0x50, 0x4b, 0x05, 0x06
    int eocdIdx;
    for (eocdIdx = buffer.length - EOCD_LEN; eocdIdx >= 0; eocdIdx--) {
      if (buffer[eocdIdx] == 0x50 && bbuf.getInt(eocdIdx) == EOCD_SIGNATURE) {
        if (LOGV) {
          Log.v(LOG_TAG, "+++ Found EOCD at index: " + eocdIdx);
        }
        break;
      }
    }
    if (eocdIdx < 0) {
      Log.d(LOG_TAG, "Zip: EOCD not found, " + zipFileName + " is not zip");
    }
    /*
     * Grab the CD offset and size, and the number of entries in the
     * archive. After that, we can release our EOCD hunt buffer.
     */
    int numEntries = bbuf.getShort(eocdIdx + EOCD_NUM_ENTRIES);
    long dirSize = bbuf.getInt(eocdIdx + EOCD_SIZE) & 0xffffffffL;
    long dirOffset = bbuf.getInt(eocdIdx + EOCD_FILE_OFFSET) & 0xffffffffL;
    // Verify that they look reasonable.
    if (dirOffset + dirSize > fileLength) {
      Log.w(
          LOG_TAG,
          "bad offsets (dir " + dirOffset + ", size " + dirSize + ", eocd " + eocdIdx + ")");
      throw new IOException();
    }
    if (numEntries == 0) {
      Log.w(LOG_TAG, "empty archive?");
      throw new IOException();
    }
    if (LOGV) {
      Log.v(
          LOG_TAG,
          "+++ numEntries=" + numEntries + " dirSize=" + dirSize + " dirOffset=" + dirOffset);
    }
    MappedByteBuffer directoryMap =
        f.getChannel().map(FileChannel.MapMode.READ_ONLY, dirOffset, dirSize);
    directoryMap.order(ByteOrder.LITTLE_ENDIAN);
    byte[] tempBuf = new byte[0xffff];
    /*
     * Walk through the central directory, adding entries to the hash table.
     */
    int currentOffset = 0;
    /*
     * Allocate the local directory information
     */
    ByteBuffer buf = ByteBuffer.allocate(LFH_LEN);
    buf.order(ByteOrder.LITTLE_ENDIAN);
    for (int i = 0; i < numEntries; i++) {
      if (directoryMap.getInt(currentOffset) != CDE_SIGNATURE) {
        Log.w(LOG_TAG, "Missed a central dir sig (at " + currentOffset + ")");
        throw new IOException();
      }
      /* useful stuff from the directory entry */
      int fileNameLen = directoryMap.getShort(currentOffset + CDE_NAME_LEN) & 0xffff;
      int extraLen = directoryMap.getShort(currentOffset + CDE_EXTRA_LEN) & 0xffff;
      int commentLen = directoryMap.getShort(currentOffset + CDE_COMMENT_LEN) & 0xffff;
      /* get the CDE filename */
      directoryMap.position(currentOffset + CDE_LEN);
      directoryMap.get(tempBuf, 0, fileNameLen);
      directoryMap.position(0);
      /* UTF-8 on Android */
      String str = new String(tempBuf, 0, fileNameLen);
      if (LOGV) {
        Log.v(LOG_TAG, "Filename: " + str);
      }
      ZipEntryRO ze = new ZipEntryRO(zipFileName, file, str);
      ze.mMethod = directoryMap.getShort(currentOffset + CDE_METHOD) & 0xffff;
      ze.mWhenModified = directoryMap.getInt(currentOffset + CDE_MOD_WHEN) & 0xffffffffL;
      ze.mCRC32 = directoryMap.getLong(currentOffset + CDECRC) & 0xffffffffL;
      ze.mCompressedLength = directoryMap.getLong(currentOffset + CDE_COMP_LEN) & 0xffffffffL;
      ze.mUncompressedLength = directoryMap.getLong(currentOffset + CDE_UNCOMP_LEN) & 0xffffffffL;
      ze.mLocalHdrOffset = directoryMap.getInt(currentOffset + CDE_LOCAL_OFFSET) & 0xffffffffL;
      // set the offsets
      buf.clear();
      ze.setOffsetFromFile(f, buf);
      // put file into hash
      mHashMap.put(str, ze);
      // go to next directory entry
      currentOffset += CDE_LEN + fileNameLen + extraLen + commentLen;
    }
    if (LOGV) {
      Log.v(LOG_TAG, "+++ zip good scan " + numEntries + " entries");
    }
  }

  private static int swapEndian(int i) {
    return ((i & 0xff) << 24) + ((i & 0xff00) << 8) + ((i & 0xff0000) >>> 8) + ((i >>> 24) & 0xff);
  }

  // 2-byte number.
  private static int swapEndian(short i) {
    return ((i & 0x00FF) << 8 | (i & 0xFF00) >>> 8);
  }
}
