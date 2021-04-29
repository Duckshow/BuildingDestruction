using System;

public readonly struct VoxelAddress : IEquatable<VoxelAddress> {
    public readonly int BinIndex;
    public readonly int LocalVoxelIndex;

    public VoxelAddress(int binIndex, int localVoxelIndex) {
        BinIndex = binIndex;
        LocalVoxelIndex = localVoxelIndex;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }

    public bool Equals(VoxelAddress other) {
        return BinIndex == other.BinIndex && LocalVoxelIndex == other.LocalVoxelIndex;
    }

    public static bool operator ==(VoxelAddress lhs, VoxelAddress rhs) => lhs.Equals(rhs);
    public static bool operator !=(VoxelAddress lhs, VoxelAddress rhs) => !(lhs == rhs);

    public override int GetHashCode() {
        int hashCode = -1623445470;
        hashCode = hashCode * -1521134295 + BinIndex.GetHashCode();
        hashCode = hashCode * -1521134295 + LocalVoxelIndex.GetHashCode();
        return hashCode;
    }

    public override string ToString() {
        return string.Format("(BinIndex: {0}, LocalVoxelIndex: {1})", BinIndex, LocalVoxelIndex);
    }
}