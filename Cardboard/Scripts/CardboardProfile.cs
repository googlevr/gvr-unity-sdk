using System;
using UnityEngine;

// Measurements of a particular phone in a particular Cardboard device.
public class CardboardProfile {
  public CardboardProfile Clone() {
    return new CardboardProfile {
      screen = this.screen,
      device = this.device
    };
  }

  // Information about the screen.  All distances in meters, measured as the phone is expected
  // to be placed in the Cardboard, i.e. landscape orientation.
  public struct Screen {
    public float width;   // The long edge of the phone.
    public float height;  // The short edge of the phone.
    public float border;  // Distance from bottom of the cardboard to the bottom edge of screen.
  }

  // Information about the lens placement in the Cardboard.  All distances in meters.
  public struct Lenses {
    public float separation;     // Center to center.
    public float offset;         // Offset of lens center from top or bottom of cardboard.
    public float screenDistance; // Distance from lens center to the phone screen.

    public int alignment;  // Determines whether lenses are placed relative to top, bottom or
                           // center.  It is actually a signum (-1, 0, +1) relating the scale of
                           // the offset's coordinates to the device coordinates.

    public const int AlignTop = -1;    // Offset is measured down from top of device.
    public const int AlignCenter = 0;  // Center alignment ignores offset, hence scale is zero.
    public const int AlignBottom = 1;  // Offset is measured up from bottom of device.
  }

  // Information about the viewing angles through the lenses.  All angles in degrees, measured
  // away from the optical axis, i.e. angles are all positive.  It is assumed that left and right
  // eye FOVs are mirror images, so that both have the same inner and outer angles.  Angles do not
  // need to account for the limits due to screen size.
  public struct MaxFOV {
    public float outer;  // Towards the side of the screen.
    public float inner;  // Towards the center line of the screen.
    public float upper;  // Towards the top of the screen.
    public float lower;  // Towards the bottom of the screen.
  }

  // Information on how the lens distorts light rays.  Also used for the (approximate) inverse
  // distortion.  Assumes a radially symmetric pincushion/barrel distortion model.
  public struct Distortion {
    public float k1;
    public float k2;

    public float distort(float r) {
      float r2 = r * r;
      return ((k2 * r2 + k1) * r2 + 1) * r;
    }
  }

  public struct Device {
    public Lenses lenses;
    public MaxFOV maxFOV;
    public Distortion distortion;
    public Distortion inverse;
  }

  // The combined set of information about a Cardboard profile.
  public Screen screen;
  public Device device;

  // The vertical offset of the lens centers from the screen center.
  public float VerticalLensOffset {
    get {
      return (device.lenses.offset - screen.border - screen.height/2) * device.lenses.alignment;
    }
  }

  // Some known profiles.

  public enum ScreenSizes {
    Nexus5,
  };

  public static readonly Screen Nexus5 = new Screen {
    width = 0.110f,
    height = 0.062f,
    border = 0.003f
  };

  public enum DeviceTypes {
    CardboardV1
  };

  public static readonly Device CardboardV1 = new Device {
    lenses = {
      separation = 0.060f,
      offset = 0.035f,
      screenDistance = 0.042f,
      alignment = Lenses.AlignBottom,
    },
    maxFOV = {
      outer = 40.0f,
      inner = 40.0f,
      upper = 40.0f,
      lower = 40.0f
    },
    distortion = {
      k1 = 0.441f,
      k2 = 0.156f
    },
    inverse = {
      k1 = -0.423f,
      k2 = 0.239f
    }
  };

  // Nexus 5 in a v1 Cardboard.
  public static readonly CardboardProfile Default = new CardboardProfile {
    screen = Nexus5,
    device = CardboardV1
  };

  public static CardboardProfile GetKnownProfile(ScreenSizes screenSize, DeviceTypes deviceType) {
    Screen screen;
    switch (screenSize) {
      default:
        screen = Nexus5;
        break;
    }
    Device device;
    switch (deviceType) {
      default:
        device = CardboardV1;
        break;
    }
    return new CardboardProfile { screen = screen, device = device };
  }
}
