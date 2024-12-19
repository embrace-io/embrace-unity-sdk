//
//  StartArgService.m
//  Unity-iPhone
//
//  Created by Chris Gaudino on 4/24/23.
//

#import <Foundation/Foundation.h>

const char *ios_getStartArguments() {
    NSString* joinedArgs = [[[NSProcessInfo processInfo] arguments] componentsJoinedByString: @" "];
    const char* joinedArgsPtr = [joinedArgs UTF8String];
    char* buffer = (char*)malloc(strlen(joinedArgsPtr) + 1);
    strcpy(buffer, joinedArgsPtr);
    return buffer;
}
