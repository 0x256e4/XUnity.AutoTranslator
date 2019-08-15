﻿using System;
using System.IO;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.ResourceRedirection
{
   /// <summary>
   /// Base implementation of resource redirect handler that takes care of the plumming for a
   /// resource redirector that is interested in either updating of dumping redirected resources.
   /// </summary>
   /// <typeparam name="TAsset">The type of asset being redirected.</typeparam>
   public abstract class AssetLoadedHandlerBase<TAsset>
      where TAsset : UnityEngine.Object
   {
      /// <summary>
      /// Constructs the resource redirection handler.
      /// </summary>
      public AssetLoadedHandlerBase()
      {
         ResourceRedirector.ResourceRedirection.RegisterAssetLoadedFor<TAsset>( Handle );
      }

      /// <summary>
      /// Gets a bool indicating if resource dumping is enabled in the Auto Translator.
      /// </summary>
      public bool IsDumpingEnabled => Settings.EnableDumping;

      /// <summary>
      /// Method to be invoked whenever a resource of the specified type is redirected.
      /// </summary>
      /// <param name="context">A context containing all relevant information of the resource redirection.</param>
      public void Handle( AssetLoadedContext<TAsset> context )
      {
         if( context.HasReferenceBeenRedirectedBefore )
         {
            context.Handled = true;
            return;
         }

         var modificationFilePath = CalculateModificationFilePath( context );
         if( File.Exists( modificationFilePath ) ) // IO, ewww!
         {
            try
            {
               context.Handled = ReplaceOrUpdateAsset( modificationFilePath, context );
               if( context.Handled )
               {
                  XuaLogger.AutoTranslator.Debug( $"Replaced resource file: '{modificationFilePath}'." );
               }
               else
               {
                  XuaLogger.AutoTranslator.Debug( $"Did not replace resource file: '{modificationFilePath}'." );
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, $"An error occurred while loading resource file: '{modificationFilePath}'." );
            }
         }
         else if( Settings.EnableDumping )
         {
            try
            {
               context.Handled = DumpAsset( modificationFilePath, context );
               if( context.Handled )
               {
                  XuaLogger.AutoTranslator.Debug( $"Dumped resource file: '{modificationFilePath}'." );
               }
               else
               {
                  XuaLogger.AutoTranslator.Debug( $"Did not dump resource file: '{modificationFilePath}'." );
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, $"An error occurred while dumping resource file: '{modificationFilePath}'." );
            }
         }
      }

      /// <summary>
      /// Method invoked when an asset should be updated or replaced.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected abstract bool ReplaceOrUpdateAsset( string calculatedModificationPath, AssetLoadedContext<TAsset> context );

      /// <summary>
      /// Method invoked when an asset should be dumped.
      /// </summary>
      /// <param name="calculatedModificationPath">This is the modification path calculated in the CalculateModificationFilePath method.</param>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A bool indicating if the event should be considered handled.</returns>
      protected abstract bool DumpAsset( string calculatedModificationPath, AssetLoadedContext<TAsset> context );

      /// <summary>
      /// Method invoked when a new resource event is fired to calculate a unique path for the resource.
      /// </summary>
      /// <param name="context">This is the context containing all relevant information for the resource redirection event.</param>
      /// <returns>A string uniquely representing a path for the redirected resource.</returns>
      protected abstract string CalculateModificationFilePath( AssetLoadedContext<TAsset> context );
   }
}