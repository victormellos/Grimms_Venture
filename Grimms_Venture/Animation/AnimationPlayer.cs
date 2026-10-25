using GrimmsVenture.Animation;
using Microsoft.Xna.Framework;

namespace GrimmsVenture.Rendering;

/// <summary>
/// Toca uma clip e calcula as matrizes de pele (bind pose -> pose atual).
/// </summary>
public sealed class AnimationPlayer
{
    readonly SkinningData _skinning;
    Matrix[] _boneTransforms;
    readonly Matrix[] _worldTransforms;
    readonly Matrix[] _skinTransforms;

    AnimationClip? _clip;
    TimeSpan _currentTime;
    bool _looping;

    public AnimationPlayer(SkinningData skinning)
    {
        _skinning = skinning;
        _boneTransforms = skinning.BindPose.ToArray();
        int n = skinning.BindPose.Count;
        _worldTransforms = new Matrix[n];
        _skinTransforms = new Matrix[n];
    }

    public AnimationClip? CurrentClip => _clip;

    public void StartClip(AnimationClip clip, bool looping)
    {
        if (_clip == clip && _looping == looping) return;
        _clip = clip;
        _looping = looping;
        _currentTime = TimeSpan.Zero;
    }

    public void Update(TimeSpan delta, Matrix rootTransform)
    {
        if (_worldTransforms.Length == 0) return; // FBX sem skeleton: nada a animar
        if (_clip != null)
        {
            _currentTime += delta;

            if (_looping && _clip.Duration > TimeSpan.Zero)
                _currentTime = TimeSpan.FromTicks(_currentTime.Ticks % _clip.Duration.Ticks);
            else if (_currentTime > _clip.Duration)
                _currentTime = _clip.Duration;

            _boneTransforms = _skinning.BindPose.ToArray();
            foreach (Keyframe kf in _clip.Keyframes)
            {
                if (kf.Time > _currentTime) break;
                _boneTransforms[kf.Bone] = kf.Transform;
            }
        }

        _worldTransforms[0] = _boneTransforms[0] * rootTransform;
        for (int bone = 1; bone < _worldTransforms.Length; bone++)
            _worldTransforms[bone] =
                _boneTransforms[bone] * _worldTransforms[_skinning.SkeletonHierarchy[bone]];

        for (int bone = 0; bone < _skinTransforms.Length; bone++)
            _skinTransforms[bone] =
                _skinning.InverseBindPose[bone] * _worldTransforms[bone];
    }

    public Matrix[] GetSkinTransforms() => _skinTransforms;
}
