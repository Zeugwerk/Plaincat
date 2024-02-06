lexer grammar StLexerStripped;

DOT:				'.';
END_TYPE_:          E N D '_' T Y P E;
END_VAR_:			E N D '_' V A R;
END_METHOD_:		E N D '_' M E T H O D;
END_PROPERTY_:		E N D '_' P R O P E R T Y;
END_FUNCTION_BLOCK_:E N D '_' F U N C T I O N '_' B L O C K;
END_FUNCTIONBLOCK_: E N D '_' F U N C T I O N B L O C K;
END_FUNCTION_:		E N D '_' F U N C T I O N;
END_PROGRAM_:		E N D '_' P R O G R A M;
END_GET_:			'E' 'N' 'D' '_' 'G' 'E' 'T';
END_SET_:			'E' 'N' 'D' '_' 'S' 'E' 'T';
SET_:				'S' 'E' 'T';
GET_:				'G' 'E' 'T';
END_IMPLEMENTATION_:E N D '_' I M P L E M E N T A T I O N; // needed for property set/get atm, todo: check what the iec syntax is here
PROGRAM_:           P R O G R A M;
FUNCTION_BLOCK_:    F U N C T I O N '_' B L O C K;
FUNCTIONBLOCK_:     F U N C T I O N B L O C K;
FUNCTION_:          F U N C T I O N;
METHOD_:            M E T H O D;
INTERFACE_:         I N T E R F A C E;
VAR_GLOBAL_:        V A R '_' G L O B A L;
PROPERTY_:          P R O P E R T Y;
INTERNAL_:          I N T E R N A L;
CONSTANT_:          C O N S T A N T;
ABSTRACT_:          A B S T R A C T;
FINAL_:             F I N A L;
PRIVATE_:           P R I V A T E;
PROTECTED_:         P R O T E C T E D;
PUBLIC_:            P U B L I C;
TYPE_ :             T Y P E;

STATIC_STRING : '\'' QUOTED_TEXT? '\'' ;
STATIC_WSTRING : '"' QUOTED_TEXT? '"' ;
fragment QUOTED_TEXT : QUOTED_TEXT_ITEM+ ;
fragment QUOTED_TEXT_ITEM
  : ESCAPED_CHARACTER
  | ~['\n\r]
  ;
fragment
ESCAPED_CHARACTER
  : '$' [$'LNPRT]
  | '$' HEXADECIMAL_DIGIT HEXADECIMAL_DIGIT
  ;

IDENTIFIER : IDENTIFIER_START IDENTIFIER_CHARACTERS ;
fragment IDENTIFIER_START : [A-Za-z] | ('_' IDENTIFIER_CHARACTERS ) ;
fragment IDENTIFIER_CHARACTERS : [A-Za-z0-9_]* ;

HEXADECIMAL : '16#' HEXADECIMAL_DIGIT HEXADECIMAL_CHARACTERS? ;
fragment HEXADECIMAL_DIGIT : [0-9] | A | B | C | D | E | F;
fragment HEXADECIMAL_CHARACTER : HEXADECIMAL_DIGIT | '_'  ;
fragment HEXADECIMAL_CHARACTERS : HEXADECIMAL_CHARACTER+ ;

NEWLINE
  : '\r'? '\n'
  ;

WS
  : [ \t\f]+ -> channel(HIDDEN)
  ;

LINE_COMMENT
  : '//' ~[\r\n]*;

BLOCK_COMMENT : '(*' (BLOCK_COMMENT|.)*? '*)' ;
  
PRAGMA : '{' .*? '}';

fragment A: [aA];
fragment B: [bB];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment G: [gG];
fragment H: [hH];
fragment I: [iI];
fragment J: [jJ];
fragment K: [kK];
fragment L: [lL];
fragment M: [mM];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment Q: [qQ];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];
fragment X: [xX];
fragment Y: [yY];
fragment Z: [zZ];

ANY : .;