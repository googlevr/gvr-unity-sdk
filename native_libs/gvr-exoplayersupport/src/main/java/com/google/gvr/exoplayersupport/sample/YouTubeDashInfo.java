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

import android.os.AsyncTask;
import android.util.Log;

import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLDecoder;
import java.nio.charset.Charset;

/**
 * Helper class to retrieve the dash info for a YouTube video. This is used to display the 360
 * degree Jump video.
 *
 * <p>This is an abstract class that loads the information for the video, then calls
 * onPostExecute(). Implementations of that method can check the isCanceled() property to make sure
 * the information is retrieved correctly, then proceed to display the video.
 */
public abstract class YouTubeDashInfo {
  private static final String TAG = "youtubedashinfo";
  private final String key;
  private String url;
  private String id;
  private boolean canceled;

  /**
   * Create the object.
   *
   * @param key - this is the key to the YouTube video found on the URL to the video in the browser.
   */
  public YouTubeDashInfo(String key) {
    this.key = key;
  }

  /** Starts the async task to retrieve and parse the information. */
  public void execute() {

    AsyncTask task =
        new AsyncTask<Object, Void, YouTubeDashInfo>() {

          final YouTubeDashInfo owner = YouTubeDashInfo.this;

          /**
           * Override this method to perform a computation on a background thread. The specified
           * parameters are the parameters passed to {@link #execute} by the caller of this task.
           *
           * <p>This method can call {@link #publishProgress} to publish updates on the UI thread.
           *
           * @param params The parameters of the task.
           * @return A result, defined by the subclass of this task.
           * @see #onPreExecute()
           * @see #onPostExecute
           * @see #publishProgress
           */
          @Override
          protected YouTubeDashInfo doInBackground(Object... params) {
            YouTubeDashInfo info = (YouTubeDashInfo) params[0];
            info.update();
            return info;
          }

          /**
           * Applications should preferably override {@link #onCancelled(Object)}. This method is
           * invoked by the default implementation of {@link #onCancelled(Object)}.
           *
           * <p>
           *
           * <p>Runs on the UI thread after {@link #cancel(boolean)} is invoked and {@link
           * #doInBackground(Object[])} has finished.
           *
           * @see #onCancelled(Object)
           * @see #cancel(boolean)
           * @see #isCancelled()
           */
          @Override
          protected void onCancelled() {
            super.onCancelled();
            owner.setCanceled(true);
            owner.onPostExecute();
          }

          /**
           * Runs on the UI thread after {@link #doInBackground}. The specified result is the value
           * returned by {@link #doInBackground}.
           *
           * <p>
           *
           * <p>This method won't be invoked if the task was cancelled.
           *
           * @param dashInfo The result of the operation computed by {@link #doInBackground}.
           * @see #onPreExecute
           * @see #doInBackground
           * @see #onCancelled(Object)
           */
          @Override
          protected void onPostExecute(YouTubeDashInfo dashInfo) {
            super.onPostExecute(dashInfo);
            dashInfo.onPostExecute();
          }
        };

    // kick off the async task here.
    task.execute(this);
  }

  /** This is called when the background processing is completed. */
  protected abstract void onPostExecute();

  /** Method to retrieve the information from YouTube and parse it. */
  private void update() {

    URL url;
    try {
      url = new URL("http://www.youtube.com/get_video_info?&video_id=" + key);
    } catch (MalformedURLException e) {
      Log.e(TAG, "Exception parsing url", e);
      setCanceled(true);
      return;
    }
    HttpURLConnection urlConnection;
    try {
      urlConnection = (HttpURLConnection) url.openConnection();
      urlConnection.setInstanceFollowRedirects(true);
    } catch (IOException e) {
      Log.e(TAG, "Exception getting " + url, e);
      setCanceled(true);
      return;
    }
    try {
      InputStream in = new BufferedInputStream(urlConnection.getInputStream());
      readStream(in);
    } catch (IOException e) {
      Log.e(TAG, "Exception reading response of " + url, e);
      setCanceled(true);
    } finally {
      urlConnection.disconnect();
    }
  }

  /**
   * Method to parse the response from YouTube. We are only interested in the dashmpd information,
   * so we skip all the other, and return when we have found what we need.
   *
   * @param in - the inputstream containing the response.
   * @throws IOException - when there is a problem.
   */
  private void readStream(InputStream in) throws IOException {
    int len = 16 * 1024;
    byte[] buf = new byte[len];
    int read = 1;
    int tot = 0;
    String s = "";
    while (read > 0) {
      read = in.read(buf);
      if (read > 0) {
        tot += read;
        s += new String(buf, 0, read);
      }
    }
    s = URLDecoder.decode(s, Charset.defaultCharset().name());
    String[] parts = s.split("&");
    url = "read " + tot + " bytes";
    for (String p : parts) {
      if (p.startsWith("dashmpd=")) {
        String val = p.substring("dashmpd=".length());
        url = URLDecoder.decode(val, Charset.defaultCharset().name());

        // pull out the content ID also.
        int idx = url.indexOf("/id/");
        if (idx >= 0) {
          String id = url.substring(idx);
          idx = id.length() > 4 ? id.indexOf("/", 5) : -1;
          if (idx > 0) {
            id = id.substring(4, id.indexOf("/", 5));
            this.id = id;
            return;
          }
        }
        // if we're not done, keep reading the input stream.
        break;
      }
    }
  }

  /**
   * The URL to the video stream.
   *
   * @return non-null once execute() has completed successfully.
   */
  public String getUrl() {
    return url;
  }

  /**
   * The content ID of the video stream.
   *
   * @return non-null once execute() has completed successfully.
   */
  public String getId() {
    return id;
  }

  protected void setCanceled(boolean canceled) {
    this.canceled = canceled;
  }

  /** @return true if processing of the request was canceled. */
  public boolean isCanceled() {
    return canceled;
  }
}
