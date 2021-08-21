﻿using System;
using UnhollowerBaseLib;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class Il2CppTypeCastingHelper : ITypeCastingHelper
   {
      public bool TryCastTo<TObject>( object obj, out TObject castedObject )
      {
         if( obj is TObject c )
         {
            castedObject = c;
            return true;
         }

         if( obj is Il2CppObjectBase il2cppObject )
         {
            IntPtr nativeClassPtr = Il2CppClassPointerStore<TObject>.NativeClassPtr;
            if( nativeClassPtr == IntPtr.Zero )
            {
               throw new ArgumentException( $"{typeof( TObject )} is not al Il2Cpp reference type" );
            }

            var instancePointer = il2cppObject.Pointer;
            IntPtr intPtr = UnhollowerBaseLib.IL2CPP.il2cpp_object_get_class( instancePointer );
            if( !UnhollowerBaseLib.IL2CPP.il2cpp_class_is_assignable_from( nativeClassPtr, intPtr ) )
            {
               castedObject = default;
               return false;
            }
            if( RuntimeSpecificsStore.IsInjected( intPtr ) )
            {
               castedObject = (TObject)UnhollowerBaseLib.Runtime.ClassInjectorBase.GetMonoObjectFromIl2CppPointer( instancePointer );
               return castedObject != null;
            }

            

            castedObject = Il2CppUtilities.Factory<TObject>.CreateProxyComponent( instancePointer );
            return castedObject != null;
         }
         else
         {
            castedObject = default;
            return false;
         }
      }
   }
}
