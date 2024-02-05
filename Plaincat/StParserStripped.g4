parser grammar StParserStripped;

options {
    tokenVocab = StLexerStripped;
}

any: (ANY | IDENTIFIER | DOT | STATIC_STRING | STATIC_WSTRING | HEXADECIMAL | WS | NEWLINE)+;

declaration: any | (any? END_VAR_)+;

implementation: any? END_IMPLEMENTATION_;

global_var_name: IDENTIFIER ;

identifier_dotted: IDENTIFIER ( DOT IDENTIFIER )* ;

function_name: derived_function_name ;

derived_function_name: IDENTIFIER ; 

derived_function_block_name: IDENTIFIER ; 

global_var_declarations: VAR_GLOBAL_ derived_function_name any? ;

function_declaration: 
    FUNCTION_
    function_declaration_modifiers?
    derived_function_name;

function_declaration_modifiers: (INTERNAL_)+ ;

interface_declaration:
  INTERFACE_
  interface_declaration_modifiers?
  derived_function_block_name
  ;

interface_declaration_modifiers: (INTERNAL_)+;

function_block_declaration: 
    (FUNCTION_BLOCK_ | FUNCTIONBLOCK_)
    function_block_declaration_modifiers?
    derived_function_block_name 
    ;

function_block_declaration_modifiers: (INTERNAL_ | ABSTRACT_ | FINAL_ | PUBLIC_)+; // public should not be allowed, but it is

method_name: IDENTIFIER ;

method_declaration:
    METHOD_
    method_declaration_modifiers*
    derived_function_name
    ;

method_declaration_modifiers: (INTERNAL_ | ABSTRACT_ | FINAL_ | PRIVATE_ | PROTECTED_ | PUBLIC_);

property_declaration:
    PROPERTY_
    property_declaration_modifier*
    derived_function_name
    ;

property_declaration_modifier: (INTERNAL_ | ABSTRACT_ | FINAL_ | PRIVATE_ | PROTECTED_ | PUBLIC_);

program_type_name: IDENTIFIER ;

program_declaration: PROGRAM_ (INTERNAL_)* program_type_name any?;

data_type_name: IDENTIFIER ;

data_type_declaration: TYPE_ INTERNAL_? data_type_name any END_TYPE_ any?;

header: any+ ;

// Documentation specific
content locals [int element]
   : { $element=1; } global_var
   | { $element=2; } data_type
   | { $element=3; } function
   | { $element=4; } interface
   | { $element=5; } function_block
   | { $element=6; } program
    ;

global_var: header? global_var_declarations implementation?;

data_type: header? data_type_declaration implementation?;

function: header? function_declaration declaration? implementation?;

interface: header? interface_declaration declaration? implementation? (method | property)*; 

function_block: header? function_block_declaration declaration? implementation? (method | property)* ;

method: header? method_declaration declaration? implementation?;

property: header? property_declaration declaration? implementation? declaration? implementation?;

program: header? program_declaration declaration? implementation? (method | property)*;
